# SimpleStack.Orm

[![Build Status](https://img.shields.io/nuget/dt/simplestack.orm)](https://www.nuget.org/packages/SimpleStack.Orm)
[![Build Status](https://img.shields.io/github/license/simplestack/simplestack.orm)](https://www.github.com/simplestack/simplestack.orm)
[![GitHub release (latest by date)](https://img.shields.io/github/v/release/simplestack/simplestack.orm)](https://github.com/SimpleStack/simplestack.orm/releases/)
![GitHub top language](https://img.shields.io/github/languages/top/simplestack/simplestack.orm)
![Maintenance](https://img.shields.io/maintenance/yes/2022)
[![Twitter URL](https://img.shields.io/twitter/url?label=Follow%20us&style=social&url=https%3A%2F%2Ftwitter.com%2Fsimplestackproj)](https://twitter.com/simplestackproj)

[SimpleStack.Orm](https://simplestack.org) is a layer on top of the wonderful [Dapper](https://github.com/StackExchange/dapper-dot-net/) project that generate SQL queries based on lambda expressions. It is designed to persist types with a minimal amount of intrusion and configuration. All the generated sql queries are using parameters to improve performance and security.  
  
By using Dynamic queries it is also possible to generate queries without a corresponding Type, see [Dynamic Queries](https://simplestack.org/query/select_async_dyn) for more information.  

#### Main goals:  

* Map a Type 1:1 to an RDBMS table or view.
* Create/Drop DB Table schemas using nothing but a Type. (IOTW a true code-first ORM)  
* Simplicity - typed, wrist friendly API for common data access patterns.  
* Full use of query parameters.  
* Supports multiple databases. Currently: Sql Server, Sqlite, MySql, PostgreSQL)  
* Cross Platform, based on netstandard 2.0.  
* Support connections on multiple databases from the same application  
  
In SimpleStak.Orm : **1 Class = 1 Table/View**. There are no surprising or hidden behavior.  [Attributes](https://simplestack.org/attributes) may be added on your Type to tune the queries generation (Alias, Schema, PrimaryKey, Index,...)

### Sample usage

```csharp
using SimpleStack.Orm;
using SimpleStack.Orm.SqlServer;

namespace Test{

   public class sample{

      [Alias("dogs")]
      public class Dog{
         [PrimaryKey()]
         [AutoIncrement()]
         public int Id{get; set;}
         public string Name{get; set;}
         [Alias("birth_date")]
         public DateTime? BirthDate{get; set;}
         public decimal Weight{get; set;}
         public string Breed{get; set;}
      }

      var factory = new OrmConnectionFactory(new SqlServerDialectProvider(), "server=...");
      using (var conn = factory.OpenConnection())
      {
         conn.CreateTable<Dog>();

         // INSERT INTO "dogs" ("Id", "Name", "birth_date", "Weight", "Breed" ) VALUES (@p_0, @p_1, @p_2, @p_3, @p_4)
         conn.Insert(new Dog{Name="Snoopy", BirthDate = new DateTime(1950,10,01), Weight=25.4});
         conn.Insert(new Dog{Name="Rex", Weight=45.6});
         conn.Insert(new Dog{Name="Popol", BirthDate = new DateTime(1918,09,13), Weight=2});

         // SELECT "Id", "Name", "birth_date" AS BirthDate, "Weight", "Breed"
         // FROM "dogs"
         // WHERE ("Id" = @p_0)
         // ORDER BY 1 -- ORDER BY is mandatory to use OFFSET and FETCH clause in SQLServer
         // OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY
         var rex = conn.First<Dog>(x => Id == 2);

         rex.BirthDate = new DateTime(1994,11,10);

         // UPDATE "dogs" SET "Name"=@p_0, "birth_date"=@p_1, "Weight"=@p_2, "Breed"=@p_3 WHERE "Id"=@p_4
         conn.Update(rex);

         // DELETE FROM "dogs" WHERE ("Name" = @p_0)
         conn.DeleteAll<Dog>(x => x.Name == "Popol");

         // SELECT "Name", "Breed", "Weight"
         // FROM "dogsbackup"
         // WHERE (DATEPART(year,"birth_date") = @p_0) --will be specific depending on database
         // ORDER BY "Breed" ASC,"Weight" DESC
         conn.Select<Dog>(x => {
             x.From("dogsbackup");                         // Change From clause
             x.Select(y => new {y.Name,y.Breed,y.Weight}); // Only return some fields
             x.Where(y => y.BirthDate.Value.Year == 2019);
             x.OrderBy(y => y.Breed)
              .ThenByDescending(y => y.Weight);
         });

         // SELECT AVG(Weight) AS Weight
         // FROM "dogs"
         conn.GetScalar<Dog, decimal>(x => Sql.Avg(x.Weight))
      }
   }
}
```
