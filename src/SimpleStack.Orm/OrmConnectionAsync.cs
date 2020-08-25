using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dapper;
using SimpleStack.Orm.Attributes;
using SimpleStack.Orm.Expressions.Statements.Dynamic;
using SimpleStack.Orm.Expressions.Statements.Typed;

namespace SimpleStack.Orm
{
    public partial class OrmConnection
    {
        /// <summary>
        /// Executes a Select query using dynamic query
        /// </summary>
        /// <param name="tableName">the table name</param>
        /// <param name="selectStatement">The select statement</param>
        /// <param name="flags">the command flags</param>
        /// <returns>IEnumerable of results as dynamix objects</returns>
        public async Task<IEnumerable<dynamic>> SelectAsync(string tableName,
            Action<DynamicSelectStatement> selectStatement, CommandFlags flags = CommandFlags.Buffered)
        {
            return await SelectAsync(tableName, null, selectStatement, flags);
        }

        /// <summary>
        /// Executes a Select query using dynamic query
        /// </summary>
        /// <param name="tableName">the table name</param>
        /// <param name="schemaName">the schema name</param>
        /// <param name="selectStatement">The select statement</param>
        /// <param name="flags">the command flags</param>
        /// <returns>IEnumerable of results as dynamix objects</returns>
        public async Task<IEnumerable<dynamic>> SelectAsync(string tableName, string schemaName,
            Action<DynamicSelectStatement> selectStatement, CommandFlags flags = CommandFlags.Buffered)
        {
            var statement = new DynamicSelectStatement(DialectProvider);

            selectStatement(statement.From(tableName, schemaName));

            var cmd = DialectProvider.ToSelectStatement(statement.Statement, flags);
            return await this.QueryAsync(cmd.CommandText, cmd.Parameters, cmd.Transaction, cmd.CommandTimeout,
                cmd.CommandType);
        }
        
       /// <summary>
       /// Executes a Select query using strongly typed query generation.
       /// </summary>
       /// <param name="predicate">Where expression predicate</param>
       /// <param name="flags">Command flags</param>
       /// <typeparam name="T"></typeparam>
       /// <returns></returns>
        public async Task<IEnumerable<T>> SelectAsync<T>(Expression<Func<T, bool>> predicate,
            CommandFlags flags = CommandFlags.Buffered)
        {
            var select = new TypedSelectStatement<T>(DialectProvider);
            select.Where(predicate);
            return await this.QueryAsync<T>(DialectProvider.ToSelectStatement(select.Statement, flags));
        }

        /// <summary>
        /// Executes a Select query using strongly typed query generation.
        /// </summary>
        /// <param name="expression">Select Statement action</param>
        /// <param name="flags">Command flags</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<IEnumerable<T>> SelectAsync<T>(Action<TypedSelectStatement<T>> expression,
            CommandFlags flags = CommandFlags.Buffered)
        {
            var select = new TypedSelectStatement<T>(DialectProvider);
            expression(select);
            return await this.QueryAsync<T>(DialectProvider.ToSelectStatement(select.Statement, flags));
        }

        /// <summary>
        /// Executes a Select all query.
        /// </summary>
        /// <param name="flags">Command flags</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<IEnumerable<T>> SelectAsync<T>(CommandFlags flags = CommandFlags.Buffered)
        {
            return await this.QueryAsync<T>(
                DialectProvider.ToSelectStatement(new TypedSelectStatement<T>(DialectProvider).Statement, flags));
        }

        /// <summary>
        /// Return the first result of a Select query using strongly typed query generation.
        /// </summary>
        /// <param name="predicate">Where clause</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If the query does not returns any element</exception>
        public async Task<T> FirstAsync<T>(Expression<Func<T, bool>> predicate)
        {
            var r = await SelectAsync<T>(x =>
            {
                x.Where(predicate);
                x.Limit(1);
            });
            return r.First();
        }

        /// <summary>
        /// Return the first result of a Select query using strongly typed query generation.
        /// </summary>
        /// <param name="expression">Select Statement action</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If the query does not returns any element</exception>
        public async Task<T> FirstAsync<T>(Action<TypedSelectStatement<T>> expression)
        {
            var r = await SelectAsync<T>(x =>
            {
                expression(x);
                x.Limit(1);
            });
            return r.First();
        }

        /// <summary>
        /// Return the first result or null if the query does not return any results.
        /// </summary>
        /// <param name="predicate">Where clause</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate)
        {
            var r = await SelectAsync<T>(x =>
            {
                x.Where(predicate);
                x.Limit(1);
            });
            return r.FirstOrDefault();
        }

        /// <summary>
        /// Return the first result or null if the query does not return any results.
        /// </summary>
        /// <param name="expression"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> FirstOrDefaultAsync<T>(Action<TypedSelectStatement<T>> expression)
        {
            var r = await SelectAsync<T>(x =>
            {
                expression(x);
                x.Limit(1);
            });
            return r.FirstOrDefault();
        }

        /// <summary>
        /// Execute a query that return a single value
        /// </summary>
        /// <example>
        ///     int maxAgeUnder50 = db.Scalar<Person, int>(x => Sql.Max(x.Age));
        /// </example>
        /// <param name="field">The expression that return the single value</param>
        /// <typeparam name="T">The Type specifying the target table</typeparam>
        /// <typeparam name="TKey">The Type of the result</typeparam>
        /// <returns></returns>
        public async Task<TKey> GetScalarAsync<T, TKey>(Expression<Func<T, TKey>> field)
        {
            var select = new TypedSelectStatement<T>(DialectProvider);
            select.Select(field);
            return await this.ExecuteScalarAsync<TKey>(
                DialectProvider.ToSelectStatement(select.Statement, CommandFlags.None));
        }

        /// <summary>
        ///  Execute a query that return a single value with a where predicate
        /// </summary>
        /// <example>
        ///     int maxAgeUnder50 = db.Scalar<Person, int>(x => Sql.Max(x.Age), x => x.Age < 50);
        /// </example>
        /// <param name="field">The expression that return the single value</param>
        /// <param name="predicate">the where clause predicate</param>
        /// <typeparam name="T">The Type specifying the target table</typeparam>
        /// <typeparam name="TKey">The type of the result</typeparam>
        /// <returns></returns>
        public async Task<TKey> GetScalarAsync<T, TKey>(Expression<Func<T, TKey>> field,
            Expression<Func<T, bool>> predicate)
        {
            var select = new TypedSelectStatement<T>(DialectProvider);
            select.Select(field);
            select.Where(predicate);
            return await this.ExecuteScalarAsync<TKey>(
                DialectProvider.ToSelectStatement(select.Statement, CommandFlags.None));
        }

        /// <summary>
        /// Execute a Select Count query
        /// </summary>
        /// <param name="expression">The Select Statement action</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<long> CountAsync<T>(Action<TypedSelectStatement<T>> expression)
        {
            var select = new TypedSelectStatement<T>(DialectProvider);
            expression(select);

            return await this.ExecuteScalarAsync<long>(
                DialectProvider.ToCountStatement(select.Statement, CommandFlags.None));
        }
                
        /// <summary>
        /// Execute a Select Count query
        /// </summary>
        /// <param name="expression">the where clause expression</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<long> CountAsync<T>(Expression<Func<T, bool>> expression)
        {
            return await CountAsync<T>(e => e.Where(expression));
        }

        /// <summary>
        /// Execute a Select Count query on a specific table
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <param name="expression">The Select Statement action</param>
        /// <returns></returns>
        public async Task<long> CountAsync(string tableName, Action<DynamicSelectStatement> expression)
        {
            return await CountAsync(tableName, null, expression);
        }

        /// <summary>
        /// Execute a Select Count query on a specific table
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <param name="schemaName">The schema name</param>
        /// <param name="expression">The Select Statement action</param>
        /// <returns></returns>
        public async Task<long> CountAsync(string tableName, string schemaName,
            Action<DynamicSelectStatement> expression)
        {
            var select = new DynamicSelectStatement(DialectProvider);
            expression(select.From(tableName, schemaName));

            return await this.ExecuteScalarAsync<long>(
                DialectProvider.ToCountStatement(select.Statement, CommandFlags.None));
        }

        /// <summary>
        /// Execute a Select count(*)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<long> CountAsync<T>()
        {
            return await CountAsync<T>(e => { });
        }

        /// <summary>
        /// Execute a query to update the give object in database using the <see cref="PrimaryKeyAttribute"/> to identify the record
        /// </summary>
        /// <param name="model">the object to update</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<int> UpdateAsync<T>(T model)
        {
            var s = new TypedUpdateStatement<T>(DialectProvider);
            s.ValuesOnly(model);
            var cmd = DialectProvider.ToUpdateStatement(s.Statement, CommandFlags.None);
            return await this.ExecuteScalarAsync<int>(cmd);
        }

        /// <summary>
        /// Execute a query to update only some fields of the give object in database using the <see cref="PrimaryKeyAttribute"/> to identify the record
        /// </summary>
        /// <param name="model">The object to update</param>
        /// <param name="onlyFields">Specify the fields that need to be updated</param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <returns></returns>
        public async Task<int> UpdateAsync<T, TKey>(T model, Expression<Func<T, TKey>> onlyFields)
        {
            var s = new TypedUpdateStatement<T>(DialectProvider);
            s.ValuesOnly(model, onlyFields);
            var cmd = DialectProvider.ToUpdateStatement(s.Statement, CommandFlags.None);
            return await this.ExecuteScalarAsync<int>(cmd);
        }

        /// <summary>
        /// Execute a query to update all record matching the where expression
        /// </summary>
        /// <param name="obj">the object values to update, the Property names must match the property name of <see cref="T"/></param>
        /// <param name="onlyField">Expression that return the fields that need to be updated</param>
        /// <param name="where">The where cclause</param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <returns></returns>
        public async Task<int> UpdateAllAsync<T, TKey>(object obj, Expression<Func<T, TKey>> onlyField,
            Expression<Func<T, bool>> where = null)
        {
            var s = new TypedUpdateStatement<T>(DialectProvider);
            if (where != null)
            {
                s.Where(where);
            }

            s.Values(obj, onlyField);

            var cmd = DialectProvider.ToUpdateStatement(s.Statement, CommandFlags.None);
            return await this.ExecuteScalarAsync<int>(cmd);
        }

        /// <summary>
        /// Insert an object in the database 
        /// </summary>
        /// <param name="obj">The object to insert</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="OrmException"></exception>
        public Task<int> InsertAsync<T>(T obj)
        {
            return InsertAsync<int, T>(obj);
        }

        /// <summary>
        /// Insert an object in the database 
        /// </summary>
        /// <param name="obj">The object to insert</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="OrmException"></exception>
        public async Task<TKey> InsertAsync<TKey, T>(T obj)
        {
            try
            {
                var insertStatement = new TypedInsertStatement<T>(DialectProvider);
                insertStatement.Values(obj, new List<string>());

                return await this.ExecuteScalarAsync<TKey>(
                    DialectProvider.ToInsertStatement(insertStatement.Statement, CommandFlags.None));
            }
            catch (Exception e)
            {
                throw new OrmException(e.Message, e);
            }
        }

        /// <summary>
        /// Insert multiple objects at once in the database
        /// </summary>
        /// <param name="objs">The objects to insert</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task<IEnumerable<int>> InsertAsync<T>(IEnumerable<T> objs)
        {
            return InsertAsync<int, T>(objs);
        }

        /// <summary>
        /// Insert multiple objects at once in the database
        /// </summary>
        /// <param name="objs">The objects to insert</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<IEnumerable<TKey>> InsertAsync<TKey, T>(IEnumerable<T> objs)
        {
            if (objs == null)
            {
                throw new ArgumentNullException(nameof(objs));
            }

            List<TKey> result = new List<TKey>();
            foreach (var t in objs)
            {
                //TODO: Optimize this only generating query once and use different parameters
                var insertStatement = new TypedInsertStatement<T>(DialectProvider);
                insertStatement.Values(t, new List<string>());

                result.Add(await this.ExecuteScalarAsync<TKey>(
                    DialectProvider.ToInsertStatement(insertStatement.Statement, CommandFlags.None)));
            }

            return result;
        }
        
        /// <summary>
        /// Insert only some fields of an object in the database
        /// </summary>
        /// <param name="obj">the object to insert</param>
        /// <param name="onlyFields">expression specifying the fields to insert</param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <returns></returns>
        public async Task<int> InsertOnlyAsync<T, TKey>(T obj, Expression<Func<T, TKey>> onlyFields)
            where T : new()
        {
            var insertStatement = new TypedInsertStatement<T>(DialectProvider);
            insertStatement.Values(obj, onlyFields);

            return await this.ExecuteScalarAsync<int>(
                DialectProvider.ToInsertStatement(insertStatement.Statement, CommandFlags.None));
        }

        /// <summary>
        /// Execute a Delete statement using the specified where clause
        /// </summary>
        /// <param name="where">The where clause</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<int> DeleteAllAsync<T>(Expression<Func<T, bool>> where = null)
        {
            return await DeleteAllAsync<T>(x => { x.Where(where); });
        }
        
        /// <summary>
        /// Execute a Delete statement using the specified where clause
        /// </summary>
        /// <param name="where">the where statement action</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<int> DeleteAllAsync<T>(Action<TypedWhereStatement<T>> where)
        {
            var s = new TypedDeleteStatement<T>(DialectProvider);
            where(s);
            return await this.ExecuteScalarAsync<int>(DialectProvider.ToDeleteStatement(s.Statement));
        }


        /// <summary>
        /// Delete a single object using the <see cref="PrimaryKeyAttribute"/> to identify the record
        /// </summary>
        /// <param name="obj">The object to delete</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<int> DeleteAsync<T>(T obj)
        {
            var s = new TypedDeleteStatement<T>(DialectProvider);
            s.AddPrimaryKeyWhereCondition(obj);

            return await this.ExecuteScalarAsync<int>(DialectProvider.ToDeleteStatement(s.Statement));
        }

        /// <summary>
        /// Create a Table corresponding to the specified type 
        /// </summary>
        /// <param name="dropIfExists">True to drop the table if it already exists</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="OrmException">If the table already exists and cannot be dropped</exception>
        public async Task CreateTableAsync<T>(bool dropIfExists)
        {
            if (!dropIfExists && await TableExistsAsync<T>())
            {
                throw new OrmException("Table already exists");
            }

            var tableType = typeof(T);
            await CreateTableAsync(dropIfExists, tableType);
        }

        /// <summary>
        /// Create a table corresponding to the specified type if it didn't exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task CreateTableIfNotExistsAsync<T>()
        {
            var tableType = typeof(T);
            await CreateTableAsync(false, tableType);
        }

        /// <summary>
        /// Create a schema if no exists
        /// </summary>
        /// <param name="schemaName">Name of the schema</param>
        /// <returns></returns>
        public async Task CreateSchemaIfNotExistsAsync(string schemaName)
        {
            await this.ExecuteScalarAsync(DialectProvider.GetCreateSchemaStatement(schemaName, true));
        }

        /// <summary>
        /// Create a schema
        /// </summary>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        public async Task CreateSchemaAsync(string schemaName)
        {
            await this.ExecuteScalarAsync(DialectProvider.GetCreateSchemaStatement(schemaName, false));
        }

        /// <summary>
        /// Execute a query to detect if a table already exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<bool> TableExistsAsync<T>()
        {
            var tableModelDef = typeof(T).GetModelDefinition();
            return await Task.Run(() =>
                DialectProvider.DoesTableExist(this,
                    tableModelDef.Alias ?? tableModelDef.ModelName,
                    tableModelDef.Schema));
        }

        /// <summary>
        /// Execute a query to detect if a table already exists
        /// </summary>
        /// <param name="tableName">the table name</param>
        /// <param name="schema">the schema name</param>
        /// <returns></returns>
        public async Task<bool> TableExistsAsync(string tableName, string schema = null)
        {
            return await Task.Run(() =>
                DialectProvider.DoesTableExist(this, tableName, schema));
        }

        /// <summary>
        /// Drop a table if it exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>True if table did exists and has been dropped</returns>
        public async Task<bool> DropTableIfExistsAsync<T>()
        {
            if (await TableExistsAsync<T>())
            {
                var tableModelDef = typeof(T).GetModelDefinition();
                await DropTableAsync(tableModelDef);
            }

            return false;
        }
        
        /// <summary>
        /// Return list of tables and/or view that exists in the database
        /// </summary>
        /// <param name="schemaName">The name of the schema</param>
        /// <param name="includeViews">Boolean to specify if views has to be returned as well</param>
        /// <returns></returns>
        public async Task<IEnumerable<ITableDefinition>> GetTablesAsync(string schemaName = null,
            bool includeViews = false)
        {
            return await DialectProvider.GetTableDefinitions(DbConnection, schemaName, includeViews);
        }

        /// <summary>
        /// Return the columns definition of a given table or view
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        public async Task<IEnumerable<IColumnDefinition>> GetTableColumnsAsync(string tableName,
            string schemaName = null)
        {
            return await Task.Run(() => DialectProvider.GetTableColumnDefinitions(DbConnection, tableName, schemaName));
        }
        
        private async Task CreateTableAsync(bool overwrite, Type modelType)
        {
            var modelDef = modelType.GetModelDefinition();

            var dialectProvider = DialectProvider;
            var tableName = dialectProvider.NamingStrategy.GetTableName(modelDef.ModelName);
            var tableExists = dialectProvider.DoesTableExist(this, tableName, modelDef.Schema);

            if (overwrite && tableExists)
            {
                await DropTableAsync(modelDef);
                tableExists = false;
            }

            if (!tableExists)
            {
                await this.ExecuteAsync(dialectProvider.ToCreateTableStatement(modelDef));

                var sqlIndexes = dialectProvider.ToCreateIndexStatements(modelDef);
                foreach (var sqlIndex in sqlIndexes)
                {
                    await this.ExecuteAsync(sqlIndex);
                }

                var sequenceList = dialectProvider.SequenceList(modelDef);
                if (sequenceList.Count > 0)
                {
                    foreach (var seq in sequenceList)
                    {
                        if (dialectProvider.DoesSequenceExist(this, seq) == false)
                        {
                            var seqSql = dialectProvider.ToCreateSequenceStatement(modelDef, seq);
                            await this.ExecuteAsync(seqSql);
                        }
                    }
                }
                else
                {
                    var sequences = dialectProvider.ToCreateSequenceStatements(modelDef);
                    foreach (var seq in sequences)
                    {
                        await this.ExecuteAsync(seq);
                    }
                }
            }
        }
        
        private async Task DropTableAsync(ModelDefinition modelDef)
        {
            var dropTableFks = DialectProvider.GetDropForeignKeyConstraints(modelDef);
            if (!string.IsNullOrEmpty(dropTableFks))
            {
                await this.ExecuteAsync(dropTableFks);
            }

            await this.ExecuteAsync(DialectProvider.GetDropTableStatement(modelDef));
        }
    }
}