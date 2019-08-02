dotnet restore src/SimpleStack.Orm
dotnet pack -c Release -o nuget src/SimpleStack.Orm
dotnet restore src/SimpleStack.Orm.MySQL
dotnet pack -c Release -o nuget src/SimpleStack.Orm.MySQL
dotnet restore src/SimpleStack.Orm.MySQLConnector
dotnet pack -c Release -o nuget src/SimpleStack.Orm.MySQLConnector
dotnet restore src/SimpleStack.Orm.PostgreSQL
dotnet pack -c Release -o nuget src/SimpleStack.Orm.PostgreSQL
dotnet restore src/SimpleStack.Orm.SQLite
dotnet pack -c Release -o nuget src/SimpleStack.Orm.SQLite
dotnet restore src/SimpleStack.Orm.SDSQlite
dotnet pack -c Release -o nuget src/SimpleStack.Orm.SDSQLite
dotnet restore src/SimpleStack.Orm.SQLServer
dotnet pack -c Release -o nuget src/SimpleStack.Orm.SQLServer
