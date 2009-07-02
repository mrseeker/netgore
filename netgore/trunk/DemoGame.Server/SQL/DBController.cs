using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using log4net;
using NetGore.Db;
using NetGore.Db.MySql;

namespace DemoGame.Server
{
    /// <summary>
    /// Provides an interface between all objects and all the database handling methods.
    /// </summary>
    public class DBController : IDisposable
    {
        static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        readonly DbConnectionPool _connectionPool;

        readonly DeleteItemQuery _deleteItemQuery;
        readonly DeleteUserEquippedQuery _deleteUserEquipQuery;
        readonly DeleteUserItemQuery _deleteUserItemQuery;
        readonly List<IDisposable> _disposableQueries = new List<IDisposable>();
        readonly InsertUserEquippedQuery _insertUserEquipQuery;
        readonly InsertUserItemQuery _insertUserItemQuery;
        readonly InsertUserQuery _insertUserQuery;
        readonly ItemGuidCreator _itemGuidCreator;
        readonly ReplaceItemQuery _replaceItemQuery;
        readonly SelectAlliancesQuery _selectAlliancesQuery;
        readonly SelectItemQuery _selectItemQuery;
        readonly SelectItemsQuery _selectItemsQuery;
        readonly SelectItemTemplatesQuery _selectItemTemplatesQuery;
        readonly SelectNPCTemplateDropsQuery _selectNPCTemplateDropsQuery;
        readonly SelectNPCTemplateQuery _selectNPCTemplateQuery;
        readonly SelectUserEquippedItemsQuery _selectUserEquippedItemsQuery;
        readonly SelectUserInventoryItemsQuery _selectUserInventoryItemsQuery;
        readonly SelectUserPasswordQuery _selectUserPasswordQuery;
        readonly SelectUserQuery _selectUserQuery;
        readonly UpdateItemFieldQuery _updateItemFieldQuery;
        readonly UpdateUserQuery _updateUserQuery;
        readonly UserExistsQuery _userExistsQuery;

        bool _disposed;

        public DeleteItemQuery DeleteItem
        {
            get { return _deleteItemQuery; }
        }

        public SelectNPCTemplateDropsQuery SelectNPCTemplateDrops
        {
            get { return _selectNPCTemplateDropsQuery; }
        }

        public DeleteUserEquippedQuery DeleteUserEquipped
        {
            get { return _deleteUserEquipQuery; }
        }

        public DeleteUserItemQuery DeleteUserItem
        {
            get { return _deleteUserItemQuery; }
        }

        public InsertUserQuery InsertUser
        {
            get { return _insertUserQuery; }
        }

        public InsertUserEquippedQuery InsertUserEquipped
        {
            get { return _insertUserEquipQuery; }
        }

        public InsertUserItemQuery InsertUserItem
        {
            get { return _insertUserItemQuery; }
        }

        public ItemGuidCreator ItemGuidCreator
        {
            get { return _itemGuidCreator; }
        }

        public ReplaceItemQuery ReplaceItem
        {
            get { return _replaceItemQuery; }
        }

        public SelectAlliancesQuery SelectAlliances
        {
            get { return _selectAlliancesQuery; }
        }

        public SelectItemQuery SelectItem
        {
            get { return _selectItemQuery; }
        }

        public SelectItemsQuery SelectItems
        {
            get { return _selectItemsQuery; }
        }

        public SelectItemTemplatesQuery SelectItemTemplates
        {
            get { return _selectItemTemplatesQuery; }
        }

        public SelectNPCTemplateQuery SelectNPCTemplate
        {
            get { return _selectNPCTemplateQuery; }
        }

        public SelectUserQuery SelectUser
        {
            get { return _selectUserQuery; }
        }

        public SelectUserEquippedItemsQuery SelectUserEquippedItems
        {
            get { return _selectUserEquippedItemsQuery; }
        }

        public SelectUserInventoryItemsQuery SelectUserInventoryItems
        {
            get { return _selectUserInventoryItemsQuery; }
        }

        public SelectUserPasswordQuery SelectUserPassword
        {
            get { return _selectUserPasswordQuery; }
        }

        public UpdateItemFieldQuery UpdateItemField
        {
            get { return _updateItemFieldQuery; }
        }

        public UpdateUserQuery UpdateUser
        {
            get { return _updateUserQuery; }
        }

        public UserExistsQuery UserExistsQuery
        {
            get { return _userExistsQuery; }
        }

        public DBController(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");

            // Create the connection pool
            _connectionPool = new MySqlDbConnectionPool(connectionString);

            if (log.IsInfoEnabled)
                log.InfoFormat("Database connection pool created.");

            // NOTE: It would be REALLY nice to find a better way to construct the query objects...
            // Create the query objects
            _insertUserQuery = new InsertUserQuery(_connectionPool);
            _disposableQueries.Add(_insertUserQuery);

            _updateUserQuery = new UpdateUserQuery(_connectionPool);
            _disposableQueries.Add(_updateUserQuery);

            _insertUserEquipQuery = new InsertUserEquippedQuery(_connectionPool);
            _disposableQueries.Add(_insertUserEquipQuery);

            _deleteUserEquipQuery = new DeleteUserEquippedQuery(_connectionPool);
            _disposableQueries.Add(_deleteUserEquipQuery);

            _selectItemQuery = new SelectItemQuery(_connectionPool);
            _disposableQueries.Add(_selectItemQuery);

            _insertUserItemQuery = new InsertUserItemQuery(_connectionPool);
            _disposableQueries.Add(_insertUserItemQuery);

            _deleteUserItemQuery = new DeleteUserItemQuery(_connectionPool);
            _disposableQueries.Add(_deleteUserItemQuery);

            _deleteItemQuery = new DeleteItemQuery(_connectionPool);
            _disposableQueries.Add(_deleteItemQuery);

            _replaceItemQuery = new ReplaceItemQuery(_connectionPool);
            _disposableQueries.Add(_replaceItemQuery);

            _updateItemFieldQuery = new UpdateItemFieldQuery(_connectionPool);
            _disposableQueries.Add(_updateItemFieldQuery);

            _selectItemsQuery = new SelectItemsQuery(_connectionPool);
            _disposableQueries.Add(_selectItemsQuery);

            _selectUserEquippedItemsQuery = new SelectUserEquippedItemsQuery(_connectionPool);
            _disposableQueries.Add(_selectUserEquippedItemsQuery);

            _selectUserInventoryItemsQuery = new SelectUserInventoryItemsQuery(_connectionPool);
            _disposableQueries.Add(_selectUserInventoryItemsQuery);

            _selectAlliancesQuery = new SelectAlliancesQuery(_connectionPool);
            _disposableQueries.Add(_selectAlliancesQuery);

            _selectUserQuery = new SelectUserQuery(_connectionPool);
            _disposableQueries.Add(_selectUserQuery);

            _selectUserPasswordQuery = new SelectUserPasswordQuery(_connectionPool);
            _disposableQueries.Add(_selectUserPasswordQuery);

            _selectItemTemplatesQuery = new SelectItemTemplatesQuery(_connectionPool);
            _disposableQueries.Add(_selectItemTemplatesQuery);

            _selectNPCTemplateDropsQuery = new SelectNPCTemplateDropsQuery(_connectionPool);
            _disposableQueries.Add(_selectNPCTemplateDropsQuery);

            _selectNPCTemplateQuery = new SelectNPCTemplateQuery(_connectionPool);
            _disposableQueries.Add(_selectNPCTemplateQuery);

            _userExistsQuery = new UserExistsQuery(_connectionPool);
            _disposableQueries.Add(_userExistsQuery);

            _itemGuidCreator = new ItemGuidCreator(_connectionPool);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            // Dispose of all the individual queries
            foreach (IDisposable item in _disposableQueries)
            {
                item.Dispose();
            }

            // Dispose of the DbConnectionPool
            _connectionPool.Dispose();
        }

        #endregion
    }
}