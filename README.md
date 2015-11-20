# A Dapper based ORM for .NET

## Introduction

Follow [@simplestack](http://twitter.com/servicestack) on twitter for updates.

SimpleStack.Orm is a set of light-weight C# extension methods around `System.Data.*` interfaces which is designed to persist POCO classes with a minimal amount of intrusion and configuration.

SimpleStack.Orm is based on the wonderfull [Dapper](https://github.com/StackExchange/dapper-dot-net/) project for all database acces. The SQL query generation code is based on [NServiceKit.OrmLite](https://github.com/NServiceKit/NServiceKit.OrmLite)

Main objectives:

  * Map a POCO class 1:1 to an RDBMS table, cleanly by conventions, without any attributes required.
  * Create/Drop DB Table schemas using nothing but POCO class definitions (IOTW a true code-first ORM)
  * Simplicity - typed, wrist friendly API for common data access patterns.
  * Fully parameterized queries
  * Cross platform - supports multiple dbs (currently: Sql Server, Sqlite, MySql, PostgreSQL) running on both .NET and Mono platforms.

In SimpleStak.Orm : **1 Class = 1 Table**. There should be no surprising or hidden behaviour.

Effectively this allows you to create a table from any POCO type and it should persist as expected in a DB Table with columns for each of the classes 1st level public properties.

# Install

## Depending on the database you want to target:

  - [Sql Server](https://www.nuget.org/packages/SimpleStack.Orm.SqlServer)
  - [MySql](https://www.nuget.org/packages/SimpleStack.Orm.MySQL)
  - [PostgreSQL](https://www.nuget.org/packages/SimpleStack.Orm.PostgreSQL)
  - [Sqlite](https://www.nuget.org/packages/SimpleStack.Orm.Sqlite/)

### 2 minutes example

```csharp
using SimpleStack.Orm;
using SimpleStack.Orm.SqlServer;

namespace Test{

   public class sample{

      public class Dog{
         [PrimaryKey]
         public int Id{get; set;}
         public string Name{get; set;}
         public DateTime? BirthDate{get; set;}
         public decimal Weight{get; set;}
         public string Breed{get; set;}
      }

      var factory = new OrmConnectionFactory(new SqlServerDialectProvider(), "server=...");
      using (var conn = factory.OpenConnection())
      {
         conn.CreateTable<Dog>();

         conn.Inser(new Dog{Name="Snoopy", BirthDate = new DateTime(1950,10,01), Weight=25.4});
         conn.Inser(new Dog{Name="Rex", Weight=45.6});
         conn.Inser(new Dog{Name="Rintintin", BirthDate = new DateTime(1918,09,13), Weight=2});

         var rex = conn.First<Dog>(x => Id == 2);
         rex.BirthDate = new DateTime(1994,11,10);

         conn.Update(rex);

         conn.Delete<Dog>(x => x.Name == "Rintintin");
      }
   }
}
```

## How to use SimpleStack.Orm

As SimpleStack.Orm is based on Dapper, I encourage you to have a look at [Dapper documentation](https://github.com/StackExchange/dapper-dot-net/blob/master/Readme.md).

The first thing todo is to create an OrmConnectionFactory specifying the Dialectprovider to use and the connectionstring of your database.

```csharp

var factory = new OrmConnectionFactory(new SqlServerDialectProvider(), "server=...");
using (var conn = factory.OpenConnection())
{
   //TODO use connection
}
```

The DialectProvider contains all the specific code required for each database.

## WHERE clause generation using strong type LINQ queries

### Equals, Not equals, Bigger than, Less than, Contains,...

```csharp
db.Select<Dog>(q => q.Name == "Rex"); // WHERE ("Name" = 'Rex')
db.Select<Dog>(q => q.Name != "Rex"); // WHERE ("Name" <> 'Rex')
db.Select<Dog>(q => q.Weight == 10); // WHERE ("Weight" = 10)
db.Select<Dog>(q => q.Weight > 10); // WHERE ("Weight" > 10)
db.Select<Dog>(q => q.Weight >= 10); // WHERE ("Weight" >= 10)
db.Select<Dog>(q => q.Weight < 10); // WHERE ("Weight" < 10)
db.Select<Dog>(q => q.Weight <= 10); // WHERE ("Weight" <= 10)
db.Select<Dog>(q => q.Name.Contains("R")); // WHERE ("Name" LIKE("%R%"))
db.Select<Dog>(q => q.Name.StartWith("R")); // WHERE ("Name" LIKE("R%"))
db.Select<Dog>(q => q.Name.EndWidth("R")); // WHERE ("Name" LIKE("%R"))

```

### Combine criterias with AND or OR

```csharp
// WHERE ("Name" LIKE 'R' OR "Weight" > 10)
db.Select<Dog>(q => q.Name.Contains("R") || q.Weight > 10);
// WHERE ("Name" LIKE 'R' AND "Weight" > 10)
db.Select<Dog>(q => q.Name.Contains("R") && q.Weight > 10);
```

### Sql class

#### IN Criteria

```csharp
// WHERE "Breed" In ('Beagle', 'Border Collie', 'Golden Retriever')
db.Select<Dog>(q => Sql.In(q.Breed, "Beagle", "Border Collie", "Golden Retriever"));
```

#### Date part methods
```csharp
// SELECT YEAR("BirthDate") FROM DOG
conn.GetScalar<Dog, int>(x => Sql.Month(x.BirthDate))
// SELECT "Id","Name","Breed","DareBirth","Weight" FROM DOG WHERE MONTH("BirthDate") = 10
conn.Select<Dog>(x => Sql.Month(x.BirthDate) = 10)
```
#### Aggregation function

```csharp
// SELECT MAX("BirthDate") FROM DOG
conn.GetScalar<Dog, DateTime>(x => Sql.Max(x.BirthDate))
// SELECT AVG("Weight") FROM DOG
conn.GetScalar<Dog, decimal>(x => Sql.Avg(x.Weight))
```


## INSERT, UPDATE and DELETEs

To see the behaviour of the different APIs, all examples uses this simple model

```csharp
public class Person
{
	public int Id { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public int? Age { get; set; }
}
```

### Update

The "Update" method will always update up to one row by generating the where clause using PrimaryKey definitions

```csharp
//UPDATE "Person" SET "FirstName" = 'Jimi',"LastName" = 'Hendrix',"Age" = 27 WHERE "Id" = 1
db.Update(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27});
```

To update only some columns, you can use the "onlyField" parameter

```csharp
//UPDATE "Person" SET "Age" = 27 WHERE "Id" = 1
db.Update(new Person { Id = 1, Age = 27}, x => x.Age);
//UPDATE "Person" SET "LastName" = 'Hendrix',"Age" = 27 WHERE "Id" = 1
db.Update(new Person { Id = 1, LastName = "Hendrix", Age = 27}, x => new {x.Age, x.LastName});
```

Anonymous object can also be used

```csharp
//UPDATE "Person" SET "Age" = 27 WHERE "Id" = 1
db.Update<Person>(new { Id = 1, Age = 27}, x => x.Age);
//UPDATE "Person" SET "LastName" = 'Hendrix',"Age" = 27 WHERE "Id" = 1
db.Update<Person>(new { Id = 1, LastName = "Hendrix", Age = 27}, x => new {x.Age, x.LastName});
```

### UpdateAll

The "UpdateAll" method will update rows using the specified where clause (if any).

```csharp
//UPDATE "Person" SET "FirstName" = 'JJ'
db.UpdateAll(new Person { FirstName = "JJ" }, p => p.FirstName);
//UPDATE "Person" SET "FirstName" = 'JJ' WHERE AGE > 27
db.UpdateAll(new Person { FirstName = "JJ" }, p => p.FirstName, x => x.Age > 27);
```

### INSERT

Insert a single row

```csharp
//INSERT INTO "Person" ("Id","FirstName","LastName","Age") VALUES (1,'Jimi','Hendrix',27)
db.Insert(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 });
```

Insert multiple rows

```csharp
//INSERT INTO "Person" ("Id","FirstName","LastName","Age") VALUES (1,'Jimi','Hendrix',27)
db.Insert(new []{
   new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 },
   new Person { Id = 2, FirstName = "Kurt", LastName = "Cobain", Age = 27 },
   });
```

AutoIncremented Primary Keys

if you specify a PrimaryKey as AutoIncrement, the PrimaryKey is not added in the INSERT query

```csharp
public class Person
{
   [PrimaryKey]
   [AutoIncrement]
	public int Id { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public int? Age { get; set; }
}

//INSERT INTO "Person" ("FirstName","LastName","Age") VALUES ('Jimi','Hendrix',27)
db.Insert(new Person { FirstName = "Jimi", LastName = "Hendrix", Age = 27 });

```

### Delete

The "Delete" method will always delete up to one row by generating the where clause using PrimaryKey definitions

```csharp
//DELETE FROM "Person" WHERE ("Id" = 2)
db.Delete(new Person{Id = 2, F});
```

### DeleteAll
Or an Expression Visitor:
```csharp
//DELETE FROM "Person" WHERE ("Age" = 27)
db.DeleteAll<Person>(x => x.Age = 27);
```
