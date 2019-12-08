using System;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dapper;
using Newtonsoft.Json;
using SimpleStack.Orm.Attributes;
using SimpleStack.Orm.xUnit.Tools;
using Xunit;

namespace SimpleStack.Orm.xUnit
{
    public class CreateTableTests : IDisposable
    {
        #region TestTypes

        class NoPk
        {
            public string StringCol { get; set; }
        }
        class OnePk : NoPk
        {
            [PrimaryKey]
            public string Pk { get; set; }
        }
        class TwoPk : OnePk
        {
            [PrimaryKey]
            public string Pk2 { get; set; }
        }
        class OneFk : NoPk
        {
            [ForeignKey(typeof(OnePk))]
            public string Fk { get; set; }
        }

        [Alias("withaliasrenamed")]
        class WithAlias
        {
            [Alias("columnAlias")]
            public string Column1 { get; set; }
        }
        
        
        [Alias("withaliasansschema")]
        [Schema("TS")]
        class WithSchemaAndAlias
        {
            [Alias("columnAlias")]
            public string Column1 { get; set; }
        }

        class WithAttributes
        {
            [AutoIncrement]
            [PrimaryKey]
            public int Id { get; }
            
            [Required]
            public string NotNullString { get; set; }
            
            [Default("N/A")]
            public string DefaultValueCol { get; set; }
            
            [Ignore]
            public string IgnoredCol { get; set; }
            
            [StringLength(10)]
            public string TenCharsStringCol { get; set; }
            
            public decimal DecimalCol { get; set; }
            public decimal? NullableDecimalCol { get; set; }

            [DecimalLength(4,2)]
            public decimal DecimalColLimited { get; set; }
            
            [Index(true)]
            public string UniqueIndexedString { get; set; }
            
            [Compute("2+3")]
            public int ComputedField { get; set; }
        }

        #endregion
        
        [Theory]
        [MemberData(nameof(ConnectionFactories.All), MemberType = typeof(ConnectionFactories))]
        public async Task CreateTableWithoutPrimaryKeys(OrmConnectionFactory factory)
        {
            using (var c = factory.OpenConnection())
            {
                Assert.False(await c.TableExistsAsync<NoPk>());
                await c.CreateTableAsync<NoPk>(true);
                Assert.Equal(1,await c.InsertAsync(new NoPk()));
                Assert.True(await c.TableExistsAsync<NoPk>());
            }
        }

        [Theory]
        [MemberData(nameof(ConnectionFactories.All), MemberType = typeof(ConnectionFactories))]
        public async Task CreateTableWithOnePk(OrmConnectionFactory factory)
        {
            using (var c = factory.OpenConnection())
            {
                Assert.False(await c.TableExistsAsync<OnePk>());
                await c.CreateTableAsync<OnePk>(true);
                Assert.True(await c.TableExistsAsync<OnePk>());
                Assert.Equal(1, await c.InsertAsync(new OnePk {Pk = "1"}));
                await Assert.ThrowsAsync<OrmException>(async () => await c.InsertAsync(new OnePk{Pk = "1"}));
            }
        }
        
        [Theory]
        [MemberData(nameof(ConnectionFactories.All), MemberType = typeof(ConnectionFactories))]
        public async Task CreateTableWithTwoPks(OrmConnectionFactory factory)
        {
            using (var c = factory.OpenConnection())
            {
                Assert.False(await c.TableExistsAsync<TwoPk>());
                await c.CreateTableAsync<TwoPk>(true);
                Assert.True(await c.TableExistsAsync<TwoPk>());
                Assert.Equal(1, await c.InsertAsync(new TwoPk{Pk = "1",Pk2 = "1"}));
                Assert.Equal(1, await c.InsertAsync(new TwoPk{Pk = "1",Pk2 = "2"}));
                await Assert.ThrowsAsync<OrmException>(async () => await c.InsertAsync(new TwoPk{Pk = "1", Pk2 = "2"}));
            }
        }

        [Theory]
        [MemberData(nameof(ConnectionFactories.All), MemberType = typeof(ConnectionFactories))]
        public async Task CreateTableWithForeignKey(OrmConnectionFactory factory)
        {
            using (var c = factory.OpenConnection())
            {
                Assert.False(await c.TableExistsAsync<OnePk>());
                Assert.False(await c.TableExistsAsync<OneFk>());
                
                await c.CreateTableAsync<OnePk>(true);
                await c.CreateTableAsync<OneFk>(true);
                
                Assert.True(await c.TableExistsAsync<OnePk>());
                Assert.True(await c.TableExistsAsync<OneFk>());
                Assert.Equal(1, await c.InsertAsync(new OnePk{Pk = "1"}));
                Assert.Equal(1, await c.InsertAsync(new OneFk{Fk = "1"}));
                
                await Assert.ThrowsAsync<OrmException>(async () => await c.InsertAsync(new OneFk{Fk = "2"}));
            }
        }

        [Theory]
        [MemberData(nameof(ConnectionFactories.All), MemberType = typeof(ConnectionFactories))]
        public async Task CreateTableWithAliases(OrmConnectionFactory factory)
        {
            using (var c = factory.OpenConnection())
            {
                await c.CreateTableAsync<WithAlias>(true);

                Assert.True(await c.TableExistsAsync<WithAlias>());
                
                c.Select<WithAlias>(x =>
                {
                    Assert.Equal(c.DialectProvider.GetQuotedTableName("withaliasrenamed"), x.Statement.TableName);
                    Assert.Equal(c.DialectProvider.GetQuotedColumnName("columnAlias") + " AS Column1",x.Statement.Columns[0]);
                });
            }
        }
        
        [Theory]
        [MemberData(nameof(ConnectionFactories.All), MemberType = typeof(ConnectionFactories))]
        public async Task CreateTableWithAliasAndSchema(OrmConnectionFactory factory)
        {
            using (var c = factory.OpenConnection())
            {
                await c.CreateSchemaIfNotExistsAsync("TS");
                await c.CreateTableAsync<WithSchemaAndAlias>(true);

                Assert.True(await c.TableExistsAsync<WithSchemaAndAlias>());

                Assert.NotEmpty(c.GetTablesInformation("TS", false)
                    .Select(x => x.SchemaName == "TS" && x.Name == "withaliasansschema"));
                
                c.Select<WithSchemaAndAlias>(x =>
                {
                    Assert.Equal(c.DialectProvider.GetQuotedTableName("withaliasansschema","TS"), x.Statement.TableName);
                    Assert.Equal(c.DialectProvider.GetQuotedColumnName("columnAlias") + " AS Column1",x.Statement.Columns[0]);
                });
            }
        }

        [Theory]
        [MemberData(nameof(ConnectionFactories.All), MemberType = typeof(ConnectionFactories))]
        public async Task CreateTableThatAlreadyExistThrowError(OrmConnectionFactory factory)
        {
            using (var c = await factory.OpenConnectionAsync())
            {
                await c.CreateTableAsync<NoPk>(true);
                
                Assert.True(await c.TableExistsAsync<NoPk>());
                c.Insert(new NoPk{StringCol = "1"});
                c.Insert(new NoPk{StringCol = "2"});
                c.Insert(new NoPk{StringCol = "3"});

                await Assert.ThrowsAsync<OrmException>(async () => await c.CreateTableAsync<NoPk>(false));
                
                //Ensure table has not been dropped
                Assert.True(await c.TableExistsAsync<NoPk>());
                Assert.Equal(3,await c.CountAsync<NoPk>());
            }
        }
        
        [Theory]
        [MemberData(nameof(ConnectionFactories.All), MemberType = typeof(ConnectionFactories))]
        public async Task CreateTableAndDropTable(OrmConnectionFactory factory)
        {
            using (var c = await factory.OpenConnectionAsync())
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
        [MemberData(nameof(ConnectionFactories.All), MemberType = typeof(ConnectionFactories))]
        public async Task CreateTableWithColumnsAndAttributes(OrmConnectionFactory factory)
        {
            using (var c = await factory.OpenConnectionAsync())
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
                Assert.Equal(1,r1.Id);
                Assert.Null(r1.IgnoredCol);
                Assert.Equal("N/A",r1.DefaultValueCol);
                Assert.Equal(5,r1.ComputedField);
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
                Assert.Equal(2,r2.Id);
                Assert.Equal("Not default this time",r2.DefaultValueCol);
                Assert.Equal(123,r2.NullableDecimalCol);

                // NotNullString cannot be null
                await Assert.ThrowsAsync<OrmException>(() => c.InsertAsync(new WithAttributes()));
                // UniqueIndexString duplicate
                await Assert.ThrowsAsync<OrmException>(() => c.InsertAsync(new WithAttributes{NotNullString = "1234",UniqueIndexedString = "val1"}));

                // No truncation tests with SQLite
                if (!c.DialectProvider.GetType().Name.ToLower().Contains("sqlite"))
                {
                    // Data truncated
                    await Assert.ThrowsAsync<OrmException>(() => c.InsertAsync(new WithAttributes{NotNullString = "1234",TenCharsStringCol = "1234567890A"}));
                }
            }
        }

        [Theory]
        [MemberData(nameof(ConnectionFactories.All), MemberType = typeof(ConnectionFactories))]
        public void GetTableColumnInfo(OrmConnectionFactory factory)
        {
            using (var conn = factory.OpenConnection())
            {
                conn.CreateTable<WithAttributes>(true);

                var t = conn.GetTableColumnsInformation("WithAttributes").ToList();
                
                Assert.NotNull(t);
                Assert.NotEmpty(t);
                Assert.Equal(8, t.Count);
                Assert.True(t[0].PrimaryKey);
            }
        }
        
        public void Dispose()
        {
            foreach (var factory in ConnectionFactories.All)
            {
                using (var c = ((OrmConnectionFactory) factory[0]).OpenConnection())
                {
                    c.DropTableIfExists<OneFk>();
                    c.DropTableIfExists<NoPk>();
                    c.DropTableIfExists<OnePk>();
                    c.DropTableIfExists<TwoPk>();
                    c.DropTableIfExists<WithAlias>();
                    c.DropTableIfExists<WithAttributes>();
                }
            }
        }
    }
}