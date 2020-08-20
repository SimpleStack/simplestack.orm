using System.Reflection;
using System.Runtime.CompilerServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SimpleStack.Orm")]
[assembly: AssemblyDescription("Lite ORM framework based on Dapper and Query Generation from ServiceStack/NServicekit")]

[assembly: InternalsVisibleTo("SimpleStack.Orm.MySQL")]
[assembly: InternalsVisibleTo("SimpleStack.Orm.PostgreSQL")]
[assembly: InternalsVisibleTo("SimpleStack.Orm.SQLServer")]
[assembly: InternalsVisibleTo("SimpleStack.Orm.Sqlite")]
[assembly: InternalsVisibleTo("SimpleStack.Orm.Tests")]