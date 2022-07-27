using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
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
        public Task<IEnumerable<dynamic>> SelectAsync(string tableName,
            Action<DynamicSelectStatement> selectStatement, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = new CancellationToken())
        {
            return SelectAsync(tableName, null, selectStatement, flags,cancellationToken);
        }

        /// <summary>
        /// Executes a Select query using dynamic query
        /// </summary>
        /// <param name="tableName">the table name</param>
        /// <param name="schemaName">the schema name</param>
        /// <param name="selectStatement">The select statement</param>
        /// <param name="flags">the command flags</param>
        /// <returns>IEnumerable of results as dynamix objects</returns>
        public Task<IEnumerable<dynamic>> SelectAsync(string tableName, string schemaName,
            Action<DynamicSelectStatement> selectStatement, CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = new CancellationToken())
        {
            var statement = new DynamicSelectStatement(DialectProvider);

            selectStatement(statement.From(tableName, schemaName));

            var cmd = DialectProvider.ToSelectStatement(statement.Statement, flags, cancellationToken);
            return this.QueryAsync(cmd.CommandText, cmd.Parameters, cmd.Transaction, cmd.CommandTimeout,
                cmd.CommandType);
        }
        
       /// <summary>
       /// Executes a Select query using strongly typed query generation.
       /// </summary>
       /// <param name="predicate">Where expression predicate</param>
       /// <param name="flags">Command flags</param>
       /// <typeparam name="T"></typeparam>
       /// <returns></returns>
        public Task<IEnumerable<T>> SelectAsync<T>(Expression<Func<T, bool>> predicate,
            CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = new CancellationToken())
        {
            var select = new TypedSelectStatement<T>(DialectProvider);
            select.Where(predicate);
            return this.QueryAsync<T>(DialectProvider.ToSelectStatement(select.Statement, flags,cancellationToken));
        }

        /// <summary>
        /// Executes a Select query using strongly typed query generation.
        /// </summary>
        /// <param name="expression">Select Statement action</param>
        /// <param name="flags">Command flags</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task<IEnumerable<T>> SelectAsync<T>(Action<TypedSelectStatement<T>> expression,
            CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = new CancellationToken())
        {
            var select = new TypedSelectStatement<T>(DialectProvider);
            expression(select);
            return this.QueryAsync<T>(DialectProvider.ToSelectStatement(select.Statement, flags, cancellationToken));
        }

        /// <summary>
        /// Executes a Select all query.
        /// </summary>
        /// <param name="flags">Command flags</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task<IEnumerable<T>> SelectAsync<T>(CommandFlags flags = CommandFlags.Buffered, CancellationToken cancellationToken = new CancellationToken())
        {
            return this.QueryAsync<T>(
                DialectProvider.ToSelectStatement(new TypedSelectStatement<T>(DialectProvider).Statement, flags, cancellationToken));
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
            }).ConfigureAwait(false);
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
            }).ConfigureAwait(false);
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
            }).ConfigureAwait(false);
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
            }).ConfigureAwait(false);
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
        public Task<TKey> GetScalarAsync<T, TKey>(Expression<Func<T, TKey>> field, CancellationToken cancellationToken = new CancellationToken())
        {
            var select = new TypedSelectStatement<T>(DialectProvider);
            select.Select(field);
            return this.ExecuteScalarAsync<TKey>(
                DialectProvider.ToSelectStatement(select.Statement, CommandFlags.None, cancellationToken));
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
        public Task<TKey> GetScalarAsync<T, TKey>(Expression<Func<T, TKey>> field,
            Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = new CancellationToken())
        {
            var select = new TypedSelectStatement<T>(DialectProvider);
            select.Select(field);
            select.Where(predicate);
            return this.ExecuteScalarAsync<TKey>(
                DialectProvider.ToSelectStatement(select.Statement, CommandFlags.None, cancellationToken));
        }

        /// <summary>
        /// Execute a Select Count query
        /// </summary>
        /// <param name="expression">The Select Statement action</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task<long> CountAsync<T>(Action<TypedSelectStatement<T>> expression, CancellationToken cancellationToken = new CancellationToken())
        {
            var select = new TypedSelectStatement<T>(DialectProvider);
            expression(select);

            return this.ExecuteScalarAsync<long>(
                DialectProvider.ToCountStatement(select.Statement, CommandFlags.None, cancellationToken));
        }
                
        /// <summary>
        /// Execute a Select Count query
        /// </summary>
        /// <param name="expression">the where clause expression</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task<long> CountAsync<T>(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = new CancellationToken())
        {
            return CountAsync<T>(e => e.Where(expression), cancellationToken);
        }

        /// <summary>
        /// Execute a Select Count query on a specific table
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <param name="expression">The Select Statement action</param>
        /// <returns></returns>
        public Task<long> CountAsync(string tableName, Action<DynamicSelectStatement> expression, CancellationToken cancellationToken = new CancellationToken())
        {
            return CountAsync(tableName, null, expression, cancellationToken);
        }

        /// <summary>
        /// Execute a Select Count query on a specific table
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <param name="schemaName">The schema name</param>
        /// <param name="expression">The Select Statement action</param>
        /// <returns></returns>
        public Task<long> CountAsync(string tableName, string schemaName,
            Action<DynamicSelectStatement> expression, CancellationToken cancellationToken = new CancellationToken())
        {
            var select = new DynamicSelectStatement(DialectProvider);
            expression(select.From(tableName, schemaName));

            return this.ExecuteScalarAsync<long>(
                DialectProvider.ToCountStatement(select.Statement, CommandFlags.None, cancellationToken));
        }

        /// <summary>
        /// Execute a Select count(*)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task<long> CountAsync<T>(CancellationToken cancellationToken = new CancellationToken())
        {
            return CountAsync<T>(e => { }, cancellationToken);
        }

        /// <summary>
        /// Execute a query to update the give object in database using the <see cref="PrimaryKeyAttribute"/> to identify the record
        /// </summary>
        /// <param name="model">the object to update</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task<int> UpdateAsync<T>(T model, CancellationToken cancellationToken = new CancellationToken())
        {
            var s = new TypedUpdateStatement<T>(DialectProvider);
            s.ValuesOnly(model);
            var cmd = DialectProvider.ToUpdateStatement(s.Statement, CommandFlags.None, cancellationToken);
            return this.ExecuteScalarAsync<int>(cmd);
        }

        /// <summary>
        /// Execute a query to update only some fields of the give object in database using the <see cref="PrimaryKeyAttribute"/> to identify the record
        /// </summary>
        /// <param name="model">The object to update</param>
        /// <param name="onlyFields">Specify the fields that need to be updated</param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <returns></returns>
        public Task<int> UpdateAsync<T, TKey>(T model, Expression<Func<T, TKey>> onlyFields, CancellationToken cancellationToken = new CancellationToken())
        {
            var s = new TypedUpdateStatement<T>(DialectProvider);
            s.ValuesOnly(model, onlyFields);
            var cmd = DialectProvider.ToUpdateStatement(s.Statement, CommandFlags.None, cancellationToken);
            return this.ExecuteScalarAsync<int>(cmd);
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
            Expression<Func<T, bool>> where = null, CancellationToken cancellationToken = new CancellationToken())
        {
            var s = new TypedUpdateStatement<T>(DialectProvider);
            if (where != null)
            {
                s.Where(where);
            }

            s.Values(obj, onlyField);

            var cmd = DialectProvider.ToUpdateStatement(s.Statement, CommandFlags.None, cancellationToken);
            return await this.ExecuteScalarAsync<int>(cmd);
        }

        /// <summary>
        /// Insert an object in the database 
        /// </summary>
        /// <param name="obj">The object to insert</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="OrmException"></exception>
        public Task<int> InsertAsync<T>(T obj, CancellationToken cancellationToken = new CancellationToken())
        {
            return InsertAsync<int, T>(obj, cancellationToken);
        }

        /// <summary>
        /// Insert an object in the database 
        /// </summary>
        /// <param name="obj">The object to insert</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="OrmException"></exception>
        public async Task<TKey> InsertAsync<TKey, T>(T obj, CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                var insertStatement = new TypedInsertStatement<T>(DialectProvider);
                insertStatement.Values(obj, Array.Empty<string>());
                var commandDefinition = DialectProvider.ToInsertStatement(insertStatement.Statement, CommandFlags.None, cancellationToken);
                return await this.ExecuteScalarAsync<TKey>(commandDefinition);
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
        public async Task<IEnumerable<TKey>> InsertAsync<T,TKey>(IEnumerable<T> objs, CancellationToken cancellationToken = new CancellationToken())
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
                    DialectProvider.ToInsertStatement(insertStatement.Statement, CommandFlags.None, cancellationToken)).ConfigureAwait(false));
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
        public async Task<int> InsertOnlyAsync<T, TKey>(T obj, Expression<Func<T, TKey>> onlyFields, CancellationToken cancellationToken = new CancellationToken())
            where T : new()
        {
            var insertStatement = new TypedInsertStatement<T>(DialectProvider);
            insertStatement.Values(obj, onlyFields);

            return await this.ExecuteScalarAsync<int>(
                DialectProvider.ToInsertStatement(insertStatement.Statement, CommandFlags.None, cancellationToken));
        }

        /// <summary>
        /// Execute a Delete statement using the specified where clause
        /// </summary>
        /// <param name="where">The where clause</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task<int> DeleteAllAsync<T>(Expression<Func<T, bool>> where = null, CancellationToken cancellationToken = new CancellationToken())
        {
            return DeleteAllAsync<T>(x => { x.Where(where); }, cancellationToken);
        }
        
        /// <summary>
        /// Execute a Delete statement using the specified where clause
        /// </summary>
        /// <param name="where">the where statement action</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task<int> DeleteAllAsync<T>(Action<TypedWhereStatement<T>> where, CancellationToken cancellationToken = new CancellationToken())
        {
            var s = new TypedDeleteStatement<T>(DialectProvider);
            where(s);
            return this.ExecuteScalarAsync<int>(DialectProvider.ToDeleteStatement(s.Statement, cancellationToken));
        }


        /// <summary>
        /// Delete a single object using the <see cref="PrimaryKeyAttribute"/> to identify the record
        /// </summary>
        /// <param name="obj">The object to delete</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task<int> DeleteAsync<T>(T obj, CancellationToken cancellationToken = new CancellationToken())
        {
            var s = new TypedDeleteStatement<T>(DialectProvider);
            s.AddPrimaryKeyWhereCondition(obj);
            
            return this.ExecuteScalarAsync<int>(DialectProvider.ToDeleteStatement(s.Statement, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Create a Table corresponding to the specified type 
        /// </summary>
        /// <param name="dropIfExists">True to drop the table if it already exists</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="OrmException">If the table already exists and cannot be dropped</exception>
        public async Task CreateTableAsync<T>(bool dropIfExists, CancellationToken cancellationToken = new CancellationToken())
        {
            if (!dropIfExists && await TableExistsAsync<T>(cancellationToken).ConfigureAwait(false))
            {
                throw new OrmException("Table already exists");
            }

            var tableType = typeof(T);
            await CreateTableAsync(dropIfExists, tableType, cancellationToken).ConfigureAwait(false);
        }
        
        /// <summary>
        /// Create a table based on the give Type
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="dropIfExists">True to drop the table if it already exists</param>
        /// <returns></returns>
        /// <exception cref="OrmException"></exception>
        public async Task CreateTableAsync(Type tableType, bool dropIfExists, CancellationToken cancellationToken = new CancellationToken())
        {
            if (!dropIfExists && await TableExistsAsync(tableType, cancellationToken))
            {
                throw new OrmException("Table already exists");
            }

            await CreateTableAsync(dropIfExists, tableType, cancellationToken);
        }

        /// <summary>
        /// Create a table corresponding to the specified type if it didn't exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task CreateTableIfNotExistsAsync<T>(CancellationToken cancellationToken = new CancellationToken())
        {
            var tableType = typeof(T);
            return CreateTableAsync(false, tableType, cancellationToken);
        }
        
        /// <summary>
        /// Create a table corresponding to the specified type if it didn't exists
        /// </summary>
        /// <param name="tableType"></param>
        /// <returns></returns>
        public async Task CreateTableIfNotExistsAsync(Type tableType, CancellationToken cancellationToken = new CancellationToken())
        {
            await CreateTableAsync(false, tableType, cancellationToken);
        }
        
        /// <summary>
        /// Create a schema if no exists
        /// </summary>
        /// <param name="schemaName">Name of the schema</param>
        /// <returns></returns>
        public Task CreateSchemaIfNotExistsAsync(string schemaName, CancellationToken cancellationToken = new CancellationToken())
        {
            return this.ExecuteScalarAsync(new CommandDefinition(DialectProvider.GetCreateSchemaStatement(schemaName, true),
                cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Create a schema
        /// </summary>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        public Task CreateSchemaAsync(string schemaName, CancellationToken cancellationToken = new CancellationToken())
        {
            return this.ExecuteScalarAsync(new CommandDefinition(DialectProvider.GetCreateSchemaStatement(schemaName, false),
                cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Execute a query to detect if a table already exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task<bool> TableExistsAsync<T>(CancellationToken cancellationToken = new CancellationToken())
        {
            return TableExistsAsync(typeof(T), cancellationToken);
        }
        
        /// <summary>
        /// Execute a query to detect if a table already exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task<bool> TableExistsAsync(Type tableType, CancellationToken cancellationToken = new CancellationToken())
        {
            var tableModelDef = tableType.GetModelDefinition();
            
            return TableExistsAsync(tableModelDef.Alias ?? tableModelDef.ModelName, tableModelDef.Schema,
                cancellationToken);
        }

        /// <summary>
        /// Execute a query to detect if a table already exists
        /// </summary>
        /// <param name="tableName">the table name</param>
        /// <param name="schema">the schema name</param>
        /// <returns></returns>
        public async Task<bool> TableExistsAsync(string tableName, string schema = null, CancellationToken cancellationToken = new CancellationToken())
        {
            return await this.ExecuteScalarAsync<int>(
                DialectProvider.ToTableExistStatement(tableName, schema, cancellationToken: cancellationToken)) > 0;
        }

        /// <summary>
        /// Drop a table if it exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>True if table did exists and has been dropped</returns>
        public async Task<bool> DropTableIfExistsAsync<T>(CancellationToken cancellationToken = new CancellationToken())
        {
            if (await TableExistsAsync<T>(cancellationToken))
            {
                var tableModelDef = typeof(T).GetModelDefinition();
                await DropTableAsync(tableModelDef, cancellationToken).ConfigureAwait(false);
            }

            return false;
        }
        
        /// <summary>
        /// Drop a table if it exists
        /// </summary>
        /// <param name="tableType"></param>
        /// <returns>True if table did exists and has been dropped</returns>
        public async Task<bool> DropTableIfExistsAsync(Type tableType,CancellationToken cancellationToken = new CancellationToken())
        {
            if (await TableExistsAsync(tableType, cancellationToken))
            {
                var tableModelDef = tableType.GetModelDefinition();
                await DropTableAsync(tableModelDef, cancellationToken);
            }

            return false;
        }
        
        /// <summary>
        /// Return list of tables and/or view that exists in the database
        /// </summary>
        /// <param name="schemaName">The name of the schema</param>
        /// <param name="includeViews">Boolean to specify if views has to be returned as well</param>
        /// <returns></returns>
        public Task<IEnumerable<ITableDefinition>> GetTablesAsync(string schemaName = null,
            bool includeViews = false)
        {
            return DialectProvider.GetTableDefinitions(DbConnection, schemaName, includeViews);
        }

        /// <summary>
        /// Return the columns definition of a given table or view
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        public Task<IEnumerable<IColumnDefinition>> GetTableColumnsAsync(string tableName,
            string schemaName = null)
        {
            return Task.Run(() => DialectProvider.GetTableColumnDefinitions(DbConnection, tableName, schemaName));
        }
        
        private async Task CreateTableAsync(bool overwrite, Type modelType, CancellationToken cancellationToken = new CancellationToken())
        {
            var modelDef = modelType.GetModelDefinition();

            var dialectProvider = DialectProvider;
            var tableName = dialectProvider.NamingStrategy.GetTableName(modelDef.ModelName);
            var tableExists = await TableExistsAsync(tableName, modelDef.Schema, cancellationToken);

            if (overwrite && tableExists)
            {
                await DropTableAsync(modelDef, cancellationToken).ConfigureAwait(false);
                tableExists = false;
            }

            if (!tableExists)
            {
                await this.ExecuteAsync(dialectProvider.ToCreateTableStatement(modelDef)).ConfigureAwait(false);

                var sqlIndexes = dialectProvider.ToCreateIndexStatements(modelDef);
                foreach (var sqlIndex in sqlIndexes)
                {
                    await this.ExecuteAsync(sqlIndex, cancellationToken).ConfigureAwait(false);
                }

                var sequenceList = dialectProvider.SequenceList(modelDef);
                if (sequenceList.Count > 0)
                {
                    foreach (var seq in sequenceList)
                    {
                        if (dialectProvider.DoesSequenceExist(this, seq) == false)
                        {
                            var seqSql = dialectProvider.ToCreateSequenceStatement(modelDef, seq);
                            await this.ExecuteAsync(seqSql, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    var sequences = dialectProvider.ToCreateSequenceStatements(modelDef);
                    foreach (var seq in sequences)
                    {
                        await this.ExecuteAsync(seq, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }
        
        private async Task DropTableAsync(ModelDefinition modelDef, CancellationToken cancellationToken = new CancellationToken())
        {
            var dropTableFks = DialectProvider.GetDropForeignKeyConstraints(modelDef);
            if (!string.IsNullOrEmpty(dropTableFks))
            {
                await this.ExecuteAsync(dropTableFks, cancellationToken).ConfigureAwait(false);
            }

            await this.ExecuteAsync(DialectProvider.GetDropTableStatement(modelDef), cancellationToken).ConfigureAwait(false);
        }
    }
}