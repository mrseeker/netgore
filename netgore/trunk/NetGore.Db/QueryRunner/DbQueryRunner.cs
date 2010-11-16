﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;

namespace NetGore.Db
{
    /// <summary>
    /// Runs database queries in non-blocking mode, when possible, while retaining
    /// order of execution for queries.
    /// </summary>
    public class DbQueryRunner : IDbQueryRunner
    {
        static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// How long, in milliseconds, the worker thread should sleep when the job queue is empty.
        /// This only happens when the queue is empty. If a job is run, it will check again
        /// immediately after to allow for fast flushing. This should be a relatively low value to reduce latency
        /// for executing queries. Larger latency results in a greater build-up of queries, which creates larger
        /// stalls for when executing blocking queries.
        /// </summary>
        const int _emptyQueueSleepTime = 10;

        readonly DbConnection _conn;
        readonly object _executeQuerySync = new object();
        readonly Queue<KeyValuePair<DbCommand, DbQueryBase>> _queue = new Queue<KeyValuePair<DbCommand, DbQueryBase>>();

        /// <summary>
        /// A list used to hold the queued items when dumping out the queue (to prevent generating garbage). This is going to
        /// be implicitly synchronized by the <see cref="_executeQuerySync"/> lock. Because of that, we don't have to worry
        /// about this list being used by more than one thread at a time.
        /// </summary>
        readonly List<KeyValuePair<DbCommand, DbQueryBase>> _queueDumpList = new List<KeyValuePair<DbCommand, DbQueryBase>>();

        readonly object _queueSync = new object();

        readonly Thread _workerThread;
        bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbQueryRunner"/> class.
        /// </summary>
        /// <param name="conn">The <see cref="DbConnection"/> to use.</param>
        public DbQueryRunner(DbConnection conn)
        {
            _conn = conn;

            if (Connection.State == ConnectionState.Closed || Connection.State == ConnectionState.Broken)
                Connection.Open();

            // Create and start the worker thread
            _workerThread = new Thread(WorkerThreadLoop) { Name = "DbQueryRunner worker thread" };
            _workerThread.Start();
        }

        /// <summary>
        /// Executes a queued job. Make sure you have the <see cref="_executeQuerySync"/> lock.
        /// </summary>
        /// <param name="job">The queued job. Passed by reference purely for performance reasons.</param>
        void ExecuteJob(ref KeyValuePair<DbCommand, DbQueryBase> job)
        {
            Debug.Assert(job.Key.Connection == Connection);

            try
            {
                // Execute
                job.Key.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // Exceptions must be swallowed for this to work correctly (otherwise we'll probably crash the worker thread)
                const string errmsg = "Error executing query `{0}`. Exception: {1}";
                if (log.IsErrorEnabled)
                    log.ErrorFormat(errmsg, job.Key.CommandText, ex);
                Debug.Fail(string.Format(errmsg, job.Key.CommandText, ex));
            }
            finally
            {
                // Free the command
                job.Value.ReleaseCommand(job.Key);
            }
        }

        /// <summary>
        /// Executes all jobs in the queue. You must acquire the <see cref="_executeQuerySync"/> lock before
        /// calling this, and not have the <see cref="_queueSync"/> lock.
        /// </summary>
        /// <returns>The number of items that were flushed from the queue.</returns>
        int FlushQueue()
        {
            Debug.Assert(_queueDumpList.Count == 0);

            // Grab the items in the queue and place them in a separate collection, then unlock the queue so non-blocking
            // queries can continue to be non-blocking. If we just dequeued from the queue directly and executed the items
            // one by one, we'd end up locking the queue for quite a while and non-blocking calls trying to enqueue
            // items would stall until all queries finished.
            lock (_queueSync)
            {
                if (_queue.Count > 0)
                {
                    _queueDumpList.AddRange(_queue);
                    Debug.Assert(_queueDumpList.Count == _queue.Count);
                    Debug.Assert(_queueDumpList[0].Value == _queue.Peek().Value);
                    _queue.Clear();
                }

                Debug.Assert(_queue.Count == 0);
            }

            // Execute the queue of items (if there were any)
            for (var i = 0; i < _queueDumpList.Count; i++)
            {
                var v = _queueDumpList[i];
                ExecuteJob(ref v);
            }

            var ret = _queueDumpList.Count;

            _queueDumpList.Clear();

            return ret;
        }

        /// <summary>
        /// The main loop for the worker thread.
        /// </summary>
        void WorkerThreadLoop()
        {
            // Loop while not disposed
            while (!_isDisposed)
            {
                try
                {
                    var flushCount = 0;

                    // Check the queue count without locking first (worst case scenario, this value is incorrect and we just
                    // end up moving on while the queue isn't actually empty... still no harm done)
                    if (_queue.Count > 0)
                    {
                        // Grab the _executeQuerySync lock so that we can make sure we dequeue and execute the query
                        // before anything else executes. Otherwise, we could end up dequeueing, then before we execute,
                        // someone else could come in and execute their query before we could get the lock. Since they didn't
                        // see the item in the queue, they don't know it exists, and thus we lose ordering.
                        lock (_executeQuerySync)
                        {
                            // Flush out the queue while we have the lock. The queries have to be executed in order anyways,
                            // so might as well do it all now while we have the lock, otherwise we'll just keep grabbing and
                            // releasing the lock for no good reason.
                            flushCount = FlushQueue();
                        }
                    }

                    // If we didn't have any jobs, sleep for a while
                    if (flushCount == 0)
                    {
                        Thread.Sleep(_emptyQueueSleepTime);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    const string errmsg = "Exception occured in worker thread loop. Exception: {0}";
                    if (log.IsErrorEnabled)
                        log.ErrorFormat(errmsg, ex);
                    Debug.Fail(string.Format(errmsg, ex));
                }
            }
        }

        #region IDbQueryRunner Members

        /// <summary>
        /// Gets the <see cref="DbConnection"/> used by this <see cref="IDbQueryRunner"/>.
        /// </summary>
        public DbConnection Connection
        {
            get { return _conn; }
        }

        /// <summary>
        /// Executes a query that returns a <see cref="DbDataReader"/> to read the results.
        /// This runs in blocking mode.
        /// </summary>
        /// <param name="cmd">The <see cref="DbCommand"/> to execute.</param>
        /// <returns>The <see cref="DbDataReader"/> to use to read the results.</returns>
        public DbDataReader BeginExecuteReader(DbCommand cmd)
        {
            try
            {
                // Grab the execution lock since we are going to be executing queries. We can't use the lock statement since
                // we are going to have to be holding onto the lock even after the method returns due to the usage of the reader.
                Monitor.Enter(_executeQuerySync);

                // Empty out the queue
                FlushQueue();

                // Finally, execute our job
                var ret = cmd.ExecuteReader();

                return ret;
            }
            catch (Exception ex)
            {
                const string errmsg = "Exception occured while trying to execute reader. Exception: {0}";
                if (log.IsErrorEnabled)
                    log.ErrorFormat(errmsg, ex);
                Debug.Fail(string.Format(errmsg, ex));

                // Make sure we release the lock before re-throwing the exception
                Monitor.Exit(_executeQuerySync);

                throw;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            // Wait for the worker thread to die now that _isDisposed is set
            try
            {
                _workerThread.Join();
            }
            catch (Exception ex)
            {
                const string errmsg = "Failed to join worker thread. Exception: {0}";
                if (log.IsErrorEnabled)
                    log.ErrorFormat(errmsg, ex);
                Debug.Fail(string.Format(errmsg, ex));
            }

            // Flush the queue to make sure all the remaining tasks are finished
            try
            {
                lock (_executeQuerySync)
                {
                    FlushQueue();
                }
            }
            catch (Exception ex)
            {
                const string errmsg = "Failed to flush queue on `{0}`. Exception: {1}";
                if (log.IsErrorEnabled)
                    log.ErrorFormat(errmsg, this, ex);
                Debug.Fail(string.Format(errmsg, this, ex));
            }

            // Dispose of the connection
            try
            {
                Connection.Dispose();
            }
            catch (Exception ex)
            {
                const string errmsg = "Failed to dispose DbConnection `{0}`. Exception: {1}";
                if (log.IsWarnEnabled)
                    log.WarnFormat(errmsg, Connection, ex);
                Debug.Fail(string.Format(errmsg, Connection, ex));
            }
        }

        /// <summary>
        /// Ends the execution of <see cref="IDbQueryRunner.BeginExecuteReader"/> call. This must
        /// be called once done reading to prevent deadlocks.
        /// </summary>
        /// <exception cref="System.Threading.SynchronizationLockException">This thread did not own the lock or the lock
        /// was not acquired.</exception>
        public void EndExecuteReader()
        {
            // Release the execution lock
            Monitor.Exit(_executeQuerySync);
        }

        /// <summary>
        /// Executes a query that has no return value.
        /// This runs in non-blocking mode.
        /// </summary>
        /// <param name="cmd">The <see cref="DbCommand"/> to execute.</param>
        /// <param name="queryBase">The <see cref="DbQueryBase"/> that is executing this command.</param>
        public void ExecuteNonReader(DbCommand cmd, DbQueryBase queryBase)
        {
            var v = new KeyValuePair<DbCommand, DbQueryBase>(cmd, queryBase);

            // Simply push the job into the queue
            lock (_queueSync)
            {
                _queue.Enqueue(v);
            }
        }

        /// <summary>
        /// Executes a query that returns the number of rows affected.
        /// This runs in blocking mode.
        /// </summary>
        /// <param name="cmd">The <see cref="DbCommand"/> to execute.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonReaderWithResult(DbCommand cmd)
        {
            int ret;

            // Grab the execution lock since we are going to be executing queries
            lock (_executeQuerySync)
            {
                // Empty out the queue
                FlushQueue();

                // Execute our job
                ret = cmd.ExecuteNonQuery();
            }

            return ret;
        }

        #endregion
    }
}