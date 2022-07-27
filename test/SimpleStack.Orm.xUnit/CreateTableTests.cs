using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.Data.Sqlite;
using SimpleStack.Orm.Attributes;
using SimpleStack.Orm.MySQL;
using SimpleStack.Orm.MySQLConnector;
using SimpleStack.Orm.PostgreSQL;
using SimpleStack.Orm.Sqlite;
using SimpleStack.Orm.SqlServer;
using SimpleStack.Orm.xUnit.Tools;
using Xunit;

namespace SimpleStack.Orm.xUnit
{
    public enum DialectType
    {
        MySQL,
        MySQLConnector,
        PostgreSQL,
        SQLServer,
        SqLite,
        SDSqLite,
    }
    
    public class BaseTest : IDisposable
    {
        private readonly string _sqliteTempFile = Path.GetRandomFileName();
        private readonly string _sqliteTempFile2 = Path.GetRandomFileName();

        private readonly TestcontainerDatabase _mysql = new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(new MySqlTestcontainerConfiguration
            {
                Database = "db",
                Username = "mysql",
                Password = "passwword"
            })
            .WithImage("redis:latest")
            .Build();
        private readonly TestcontainerDatabase _mysqlConnector = new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(new MySqlTestcontainerConfiguration
            {
                Database = "db",
                Username = "mysql",
                Password = "passwword"
            })
            .Build();
        private readonly TestcontainerDatabase _postgresql = new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {
                Database = "db",
                Username = "postgres",
                Password = "postgres",
            })
            .Build();
        
        private readonly TestcontainerDatabase _sqlServer = new TestcontainersBuilder<MsSqlTestcontainer>()
        .WithDatabase(new MsSqlTestcontainerConfiguration
        {
            Password = "#testingDockerPassword#"
        })
        .Build();

        protected Dictionary<DialectType, OrmConnectionFactory> _ormConnectionFactories =
            new Dictionary<DialectType, OrmConnectionFactory>();

        public BaseTest()
        {
            _mysql.StartAsync().Wait();
            _mysqlConnector.StartAsync().Wait();
            _postgresql.StartAsync().Wait();
            _sqlServer.StartAsync().Wait();
            
            _ormConnectionFactories.Add(DialectType.MySQL,new OrmConnectionFactory(new MySqlDialectProvider(), _mysql.ConnectionString));
            _ormConnectionFactories.Add(DialectType.MySQLConnector,new OrmConnectionFactory(new MySqlConnectorDialectProvider(), _mysqlConnector.ConnectionString));
            _ormConnectionFactories.Add(DialectType.PostgreSQL,new OrmConnectionFactory(new PostgreSQLDialectProvider(), _postgresql.ConnectionString));
            _ormConnectionFactories.Add(DialectType.SQLServer,new OrmConnectionFactory(new SqlServerDialectProvider(), $"{_sqlServer.ConnectionString};TrustServerCertificate=true"));
            
            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = _sqliteTempFile,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Shared
            };

            _ormConnectionFactories.Add(DialectType.SqLite,new OrmConnectionFactory(new SqliteDialectProvider(), builder.ToString()));
            _ormConnectionFactories.Add(DialectType.SDSqLite, new OrmConnectionFactory(new SDSQLite.SqliteDialectProvider(),
                $"Data Source={_sqliteTempFile2};foreign keys=true;Version=3;New=True;BinaryGUID=False"));
        }
        
        public void Dispose()
        {
            _mysql.DisposeAsync().AsTask().Wait();
            _mysqlConnector.DisposeAsync().AsTask().Wait();
            _postgresql.DisposeAsync().AsTask().Wait();
            _sqlServer.DisposeAsync().AsTask().Wait();
            
            File.Delete(_sqliteTempFile);
            File.Delete(_sqliteTempFile2);
        }

        public async Task<OrmConnection> OpenConnection(DialectType dialect)
        {
            return await _ormConnectionFactories[dialect].OpenConnectionAsync();
        }
    }

    public class CreateTableTests : IClassFixture<BaseTest>
    {
        private readonly BaseTest _baseTest;

        public CreateTableTests(BaseTest baseTest)
        {
            _baseTest = baseTest;
        }
        
        [Theory]
        [InlineData(DialectType.MySQL)]
        [InlineData(DialectType.MySQLConnector)]
        [InlineData(DialectType.PostgreSQL)]
        [InlineData(DialectType.SqLite)]
        [InlineData(DialectType.SDSqLite)]
        [InlineData(DialectType.SQLServer)]
        public async Task CreateTableWithoutPrimaryKeys(DialectType dialect)
        {
            using (var c = await _baseTest.OpenConnection(dialect))
            {
                Assert.False(await c.TableExistsAsync<NoPk>());
                await c.CreateTableAsync<NoPk>(true);
                // Assert.Equal(1, await c.InsertAsync(new NoPk()));
                Assert.True(await c.TableExistsAsync<NoPk>());
            }
        }
        
        [Theory]
        [InlineData(DialectType.MySQL)]
        [InlineData(DialectType.MySQLConnector)]
        [InlineData(DialectType.PostgreSQL)]
        [InlineData(DialectType.SqLite)]
        [InlineData(DialectType.SDSqLite)]
        [InlineData(DialectType.SQLServer)]
        public async Task CreateTableWithOnePk(DialectType dialect)
        {
            using (var c = await _baseTest.OpenConnection(dialect))
            {
                Assert.False(await c.TableExistsAsync<OnePk>());
                await c.CreateTableAsync<OnePk>(true);
                Assert.True(await c.TableExistsAsync<OnePk>());
                
                Assert.Equal(1,await c.InsertAsync(new OnePk {Pk = "1"}));
                await Assert.ThrowsAsync<OrmException>(async () => await c.InsertAsync(new OnePk {Pk = "1"}));
            }
        }

        [Theory]
        [InlineData(DialectType.MySQL)]
        [InlineData(DialectType.MySQLConnector)]
        [InlineData(DialectType.PostgreSQL)]
        [InlineData(DialectType.SqLite)]
        [InlineData(DialectType.SDSqLite)]
        [InlineData(DialectType.SQLServer)]
        public async Task CreateTableWithTwoPks(DialectType dialect)
        {
            using (var c = await _baseTest.OpenConnection(dialect))
            {
                Assert.False(await c.TableExistsAsync<TwoPk>());
                await c.CreateTableAsync<TwoPk>(true);
                Assert.True(await c.TableExistsAsync<TwoPk>());
                Assert.Equal(1, await c.InsertAsync(new TwoPk {Pk = "1", Pk2 = "1"}));
                Assert.Equal(1, await c.InsertAsync(new TwoPk {Pk = "1", Pk2 = "2"}));
                await Assert.ThrowsAsync<OrmException>(async () =>
                    await c.InsertAsync(new TwoPk {Pk = "1", Pk2 = "2"}));
            }
        }

        [Theory]
        [InlineData(DialectType.MySQL)]
        [InlineData(DialectType.MySQLConnector)]
        [InlineData(DialectType.PostgreSQL)]
        [InlineData(DialectType.SqLite)]
        [InlineData(DialectType.SDSqLite)]
        [InlineData(DialectType.SQLServer)]
        public async Task CreateTableWithForeignKey(DialectType dialect)
        {
            using (var c = await _baseTest.OpenConnection(dialect))
            {
                Assert.False(await c.TableExistsAsync<OnePk>());
                Assert.False(await c.TableExistsAsync<OneFk>());

                await c.CreateTableAsync<OnePk>(true);
                await c.CreateTableAsync<OneFk>(true);

                Assert.True(await c.TableExistsAsync<OnePk>());
                Assert.True(await c.TableExistsAsync<OneFk>());
                Assert.Equal(1, await c.InsertAsync(new OnePk {Pk = "1"}));
                Assert.Equal(1, await c.InsertAsync(new OneFk {Fk = "1"}));

                await Assert.ThrowsAsync<OrmException>(async () => await c.InsertAsync(new OneFk {Fk = "2"}));
            }
        }

        [Theory]
        [InlineData(DialectType.MySQL)]
        [InlineData(DialectType.MySQLConnector)]
        [InlineData(DialectType.PostgreSQL)]
        [InlineData(DialectType.SqLite)]
        [InlineData(DialectType.SDSqLite)]
        [InlineData(DialectType.SQLServer)]
        public async Task CreateTableWithAliases(DialectType dialect)
        {
            using (var c = await _baseTest.OpenConnection(dialect))
            {
                await c.CreateTableAsync<WithAlias>(true);

                Assert.True(await c.TableExistsAsync<WithAlias>());

                c.Select<WithAlias>(x =>
                {
                    Assert.Equal(c.DialectProvider.GetQuotedTableName("withaliasrenamed"), x.Statement.TableName);
                    Assert.Equal(c.DialectProvider.GetQuotedColumnName("columnAlias") + " AS " + c.DialectProvider.GetQuotedColumnName("Column1"),
                        x.Statement.Columns[0]);
                });
            }
        }

        [Theory]
        // [InlineData(DialectType.MySQL)]
        // [InlineData(DialectType.MySQLConnector)]
        [InlineData(DialectType.PostgreSQL)]
        [InlineData(DialectType.SqLite)]
        [InlineData(DialectType.SDSqLite)]
        [InlineData(DialectType.SQLServer)]
        public async Task CreateTableWithAliasAndSchema(DialectType dialect)
        {
            using (var c = await _baseTest.OpenConnection(dialect))
            {
                await c.CreateSchemaIfNotExistsAsync("TS");
                await c.CreateTableAsync<WithSchemaAndAlias>(true);

                Assert.True(await c.TableExistsAsync<WithSchemaAndAlias>());

                Assert.NotEmpty((await c.GetTablesAsync("TS"))
                    .Select(x => x.SchemaName == "TS" && x.Name == "withaliasansschema"));

                c.Select<WithSchemaAndAlias>(x =>
                {
                    Assert.Equal(c.DialectProvider.GetQuotedTableName("withaliasansschema", "TS"),
                        x.Statement.TableName);
                    Assert.Equal(c.DialectProvider.GetQuotedColumnName("columnAlias") + " AS " + c.DialectProvider.GetQuotedColumnName("Column1"),
                        x.Statement.Columns[0]);
                });
            }
        }

        [Theory]
        [InlineData(DialectType.MySQL)]
        [InlineData(DialectType.MySQLConnector)]
        [InlineData(DialectType.PostgreSQL)]
        [InlineData(DialectType.SqLite)]
        [InlineData(DialectType.SDSqLite)]
        [InlineData(DialectType.SQLServer)]
        public async Task CreateTableThatAlreadyExistThrowError(DialectType dialect)
        {
            using (var c = await _baseTest.OpenConnection(dialect))
            {
                await c.CreateTableAsync<NoPk>(true);

                Assert.True(await c.TableExistsAsync<NoPk>());
                c.Insert(new NoPk {StringCol = "1"});
                c.Insert(new NoPk {StringCol = "2"});
                c.Insert(new NoPk {StringCol = "3"});

                await Assert.ThrowsAsync<OrmException>(async () => await c.CreateTableAsync<NoPk>(false));

                //Ensure table has not been dropped
                Assert.True(await c.TableExistsAsync<NoPk>());
                Assert.Equal(3, await c.CountAsync<NoPk>());
            }
        }

        [Theory]
        [InlineData(DialectType.MySQL)]
        [InlineData(DialectType.MySQLConnector)]
        [InlineData(DialectType.PostgreSQL)]
        [InlineData(DialectType.SqLite)]
        [InlineData(DialectType.SDSqLite)]
        [InlineData(DialectType.SQLServer)]
        public async Task CreateTableAndDropTable(DialectType dialect)
        {
            using (var c = await _baseTest.OpenConnection(dialect))
            {
                await c.CreateTableAsync<NoPk>(false);
                Assert.True(await c.TableExistsAsync<NoPk>());

                await c.DropTableIfExistsAsync<NoPk>();
                Assert.False(await c.TableExistsAsync<NoPk>());

                await c.CreateTableAsync<NoPk>(false);
                Assert.True(await c.TableExistsAsync<NoPk>());
            }
        }

        [Theory]
        [InlineData(DialectType.MySQL)]
        [InlineData(DialectType.MySQLConnector)]
        [InlineData(DialectType.PostgreSQL)]
        [InlineData(DialectType.SqLite)]
        [InlineData(DialectType.SDSqLite)]
        [InlineData(DialectType.SQLServer)]
        public async Task CreateTableWithColumnsAndAttributes(DialectType dialect)
        {
            using (var c = await _baseTest.OpenConnection(dialect))
            {
                Assert.False(await c.TableExistsAsync<WithAttributes>());
                await c.CreateTableAsync<WithAttributes>(false);
                Assert.True(await c.TableExistsAsync<WithAttributes>());

                await c.InsertAsync(new WithAttributes
                {
                    DecimalCol = 1234.56m,
                    NotNullString = "this is not null",
                    UniqueIndexedString = "val1",
                    TenCharsStringCol = "max10chars",
                    DecimalColLimited = 90.12m,
                    IgnoredCol = "this should be ignored",
                    ComputedField = 0
                });

                var r1 = await c.FirstOrDefaultAsync<WithAttributes>(x => x.Id == 1);

                Assert.NotNull(r1);
                Assert.Equal(1, r1.Id);
                Assert.Null(r1.IgnoredCol);
                Assert.Equal("N/A", r1.DefaultValueCol);
                Assert.Equal(5, r1.ComputedField);
                Assert.Null(r1.NullableDecimalCol);

                c.Insert(new WithAttributes
                {
                    DefaultValueCol = "Not default this time",
                    UniqueIndexedString = "val2",
                    NotNullString = "this is not null either",
                    NullableDecimalCol = 123
                });

                var r2 = await c.FirstOrDefaultAsync<WithAttributes>(x => x.Id == 2);
                Assert.NotNull(r2);
                Assert.Equal(2, r2.Id);
                Assert.Equal("Not default this time", r2.DefaultValueCol);
                Assert.Equal(123, r2.NullableDecimalCol);

                // NotNullString cannot be null
                await Assert.ThrowsAsync<OrmException>(() => c.InsertAsync(new WithAttributes()));
                // UniqueIndexString duplicate
                await Assert.ThrowsAsync<OrmException>(() =>
                    c.InsertAsync(new WithAttributes {NotNullString = "1234", UniqueIndexedString = "val1"}));

                // No truncation tests with SQLite
                if (!c.DialectProvider.GetType().Name.ToLower().Contains("sqlite"))
                {
                    // Data truncated
                    await Assert.ThrowsAsync<OrmException>(() => c.InsertAsync(new WithAttributes
                        {NotNullString = "1234", TenCharsStringCol = "1234567890A"}));
                }
            }
        }

        [Theory]
        [InlineData(DialectType.MySQL)]
        [InlineData(DialectType.MySQLConnector)]
        [InlineData(DialectType.PostgreSQL)]
        [InlineData(DialectType.SqLite)]
        [InlineData(DialectType.SDSqLite)]
        [InlineData(DialectType.SQLServer)]
        public  async Task GetTableColumnInfo(DialectType dialect)
        {
            using (var conn = await _baseTest.OpenConnection(dialect))
            {
                await conn.CreateTableAsync<WithAttributes>(true);

                var t = (await conn.GetTableColumnsAsync("WithAttributes")).ToList();

                Assert.NotNull(t);
                Assert.NotEmpty(t);
                Assert.Equal(8, t.Count);
                Assert.True(t[0].PrimaryKey);
            }
        }

        #region TestTypes

        private class NoPk
        {
            public string StringCol { get; set; }
        }

        private class OnePk : NoPk
        {
            [PrimaryKey] public string Pk { get; set; }
        }

        private class TwoPk : OnePk
        {
            [PrimaryKey] public string Pk2 { get; set; }
        }

        private class OneFk : NoPk
        {
            [ForeignKey(typeof(OnePk))] public string Fk { get; set; }
        }

        [Alias("withaliasrenamed")]
        private class WithAlias
        {
            [Alias("columnAlias")] public string Column1 { get; set; }
        }


        [Alias("withaliasansschema")]
        [Schema("TS")]
        private class WithSchemaAndAlias
        {
            [Alias("columnAlias")] public string Column1 { get; set; }
        }

        private class WithAttributes
        {
            [AutoIncrement] [PrimaryKey] public int Id { get; }

            [Required] public string NotNullString { get; set; }

            [Default("N/A")] public string DefaultValueCol { get; set; }

            [Ignore] public string IgnoredCol { get; set; }

            [StringLength(10)] public string TenCharsStringCol { get; set; }

            public decimal DecimalCol { get; set; }
            public decimal? NullableDecimalCol { get; set; }

            [DecimalLength(4, 2)] public decimal DecimalColLimited { get; set; }

            [Index(true)] public string UniqueIndexedString { get; set; }

            [Compute("2+3")] public int ComputedField { get; set; }
        }

        #endregion
    }
}