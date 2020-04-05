using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;
using SimpleStack.Orm.MySQL;
using SimpleStack.Orm.MySQLConnector;
using SimpleStack.Orm.PostgreSQL;
using SimpleStack.Orm.Sqlite;
using SimpleStack.Orm.SqlServer;

namespace SimpleStack.Orm.xUnit.Tools
{
    public class ConnectionFactories
    {
        private static readonly List<OrmConnectionFactory> Factories = new List<OrmConnectionFactory>();
        
        static ConnectionFactories()
        {
            Factories.Add(new OrmConnectionFactory(new MySqlDialectProvider(),"server=localhost;user=root;password=depfac$2000;database=test"));
            Factories.Add(new OrmConnectionFactory(new MySqlConnectorDialectProvider(),"server=localhost;user=root;password=depfac$2000;database=test2"));
            Factories.Add(new OrmConnectionFactory(new SqlServerDialectProvider(), @"server=localhost;User id=sa;Password=depfac$2000;database=test"));
            Factories.Add(new OrmConnectionFactory(new PostgreSQLDialectProvider(),"server=localhost;user id=postgres;password=depfac$2000;database=test;Enlist=true"));
            
            SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder();
            builder = new SqliteConnectionStringBuilder();
            builder.DataSource = Path.Combine(Path.GetTempPath(),"test2.db");
            builder.Mode = SqliteOpenMode.ReadWriteCreate;
            builder.Cache = SqliteCacheMode.Shared;

            Factories.Add(new OrmConnectionFactory(new SqliteDialectProvider(), builder.ToString()));
            Factories.Add(new OrmConnectionFactory(new SDSQLite.SqliteDialectProvider(), $"Data Source={Path.Combine(Path.GetTempPath(),"test.db")};foreign keys=true;Version=3;New=True;BinaryGUID=False"));
        }

        public static IEnumerable<object[]> All
        {
            get
            {
                return Factories.Select(x =>  new []{x});
            }
        }
    }
}