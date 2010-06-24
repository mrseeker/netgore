using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using DemoGame.DbObjs;
using DemoGame.Server.DbObjs;
using NetGore.Db;

namespace DemoGame.Server.Queries
{
    [DbControllerQuery]
    public class UpdateMapQuery : DbQueryNonReader<IMapTable>
    {
        static readonly string _queryStr = FormatQueryString("UPDATE `{0}` SET {1} WHERE `id`=@id", MapTable.TableName,
                                                            FormatParametersIntoString(MapTable.DbNonKeyColumns));

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateMapQuery"/> class.
        /// </summary>
        /// <param name="connectionPool">The connection pool.</param>
        public UpdateMapQuery(DbConnectionPool connectionPool) : base(connectionPool, _queryStr)
        {
            QueryAsserts.ArePrimaryKeys(MapTable.DbKeyColumns, "id");
        }

        /// <summary>
        /// When overridden in the derived class, creates the parameters this class uses for creating database queries.
        /// </summary>
        /// <returns>IEnumerable of all the DbParameters needed for this class to perform database queries. If null,
        /// no parameters will be used.</returns>
        protected override IEnumerable<DbParameter> InitializeParameters()
        {
            return CreateParameters(MapTable.DbColumns);
        }

        /// <summary>
        /// When overridden in the derived class, sets the database parameters based on the specified item.
        /// </summary>
        /// <param name="p">Collection of database parameters to set the values for.</param>
        /// <param name="map">Item used to execute the query.</param>
        protected override void SetParameters(DbParameterValues p, IMapTable map)
        {
            (map).CopyValues(p);
        }
    }
}