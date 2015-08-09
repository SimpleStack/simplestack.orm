using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SimpleStack.Orm
{
	/// <summary>A join SQL builder.</summary>
	/// <typeparam name="TNewPoco"> Type of the new poco.</typeparam>
	/// <typeparam name="TBasePoco">Type of the base poco.</typeparam>
	public class JoinSqlBuilder<TNewPoco, TBasePoco>
	{
		private readonly IDialectProvider _dialectProvider;
		private IDictionary<string,object> _parameters = new Dictionary<string, object>();

		/// <summary>List of joins.</summary>
		private List<Join> joinList = new List<Join>();

		/// <summary>List of wheres.</summary>
		private List<KeyValuePair<string, WhereType>> whereList = new List<KeyValuePair<string, WhereType>>();

		/// <summary>List of columns.</summary>
		private List<string> columnList = new List<string>();

		/// <summary>List of order bies.</summary>
		private List<KeyValuePair<string, bool>> orderByList = new List<KeyValuePair<string, bool>>();

		/// <summary>true if this object is distinct.</summary>
		private bool isDistinct = false;

		/// <summary>true if this object is aggregate used.</summary>
		private bool isAggregateUsed = false;

		/// <summary>The base schema.</summary>
		private string baseSchema = "";

		/// <summary>Name of the base table.</summary>
		private string baseTableName = "";

		/// <summary>Type of the base poco.</summary>
		private Type basePocoType;

		/// <summary>
		/// Initializes a new instance of the NServiceKit.OrmLite.JoinSqlBuilder&lt;TNewPoco,
		/// TBasePoco&gt; class.
		/// </summary>
		public JoinSqlBuilder(IDialectProvider dialectProvider)
		{
			_dialectProvider = dialectProvider;
			basePocoType = typeof(TBasePoco);
			baseSchema = GetSchema(basePocoType);
			baseTableName = basePocoType.GetModelDefinition().ModelName;
		}

		public IDictionary<string, object> Parameters
		{
			get { return _parameters; }
		}

		/// <summary>Columns.</summary>
		/// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="tableName">      Name of the table.</param>
		/// <param name="func">           The function.</param>
		/// <param name="withTablePrefix">true to with table prefix.</param>
		/// <returns>A string.</returns>
		private string Column<T>(string tableName, Expression<Func<T, object>> func, bool withTablePrefix)
		{
			var lst = ColumnList<T>(tableName, func, withTablePrefix);
			if (lst == null || lst.Count != 1)
				throw new OrmException("Expression should have only one column");
			return lst[0];
		}

		/// <summary>Column list.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="tableName">      Name of the table.</param>
		/// <param name="func">           The function.</param>
		/// <param name="withTablePrefix">true to with table prefix.</param>
		/// <returns>A List&lt;string&gt;</returns>
		private List<string> ColumnList<T>(string tableName, Expression<Func<T, object>> func, bool withTablePrefix = true)
		{
			List<string> result = new List<string>();
			if (func == null || func.Body == null)
				return result;
			PropertyList<T>(tableName, func.Body, result, withTablePrefix);
			return result;
		}

		/// <summary>Column list.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="withTablePrefix">true to with table prefix.</param>
		/// <returns>A List&lt;string&gt;</returns>
		private List<string> ColumnList<T>(bool withTablePrefix = true)
		{
			var pocoType = typeof(T);
			var tableName = pocoType.GetModelDefinition().ModelName;
			List<string> result = new List<string>(pocoType.GetModelDefinition().FieldDefinitions.Count);
			foreach (var item in pocoType.GetModelDefinition().FieldDefinitions)
			{
				if (withTablePrefix)
					result.Add(string.Format("{0}.{1}", _dialectProvider.GetQuotedTableName(tableName), _dialectProvider.GetQuotedColumnName(item.FieldName)));
				else
					result.Add(string.Format("{0}", _dialectProvider.GetQuotedColumnName(item.FieldName)));
			}
			return result;
		}

		/// <summary>Process the unary.</summary>
		/// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="tableName">      Name of the table.</param>
		/// <param name="u">              The UnaryExpression to process.</param>
		/// <param name="lst">            The list.</param>
		/// <param name="withTablePrefix">true to with table prefix.</param>
		private void ProcessUnary<T>(string tableName, UnaryExpression u, List<string> lst, bool withTablePrefix)
		{
			if (u.NodeType == ExpressionType.Convert)
			{
				if (u.Method != null)
				{
					throw new OrmException("Invalid Expression provided");
				}
				PropertyList<T>(tableName, u.Operand, lst, withTablePrefix);
				return;
			}
			throw new OrmException("Invalid Expression provided");
		}

		/// <summary>Process the member access.</summary>
		/// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="tableName">      Name of the table.</param>
		/// <param name="m">              The MemberExpression to process.</param>
		/// <param name="lst">            The list.</param>
		/// <param name="withTablePrefix">true to with table prefix.</param>
		/// <param name="alias">          The alias.</param>
		protected void ProcessMemberAccess<T>(string tableName, MemberExpression m, List<string> lst, bool withTablePrefix, string alias = "")
		{
			if (m.Expression != null
				&& (m.Expression.NodeType == ExpressionType.Parameter || m.Expression.NodeType == ExpressionType.Convert))
			{
				var pocoType = typeof(T);
				var fieldName = pocoType.GetModelDefinition().FieldDefinitions.First(f => f.Name == m.Member.Name).FieldName;

				if (withTablePrefix)
					lst.Add(string.Format("{0}.{1}{2}", _dialectProvider.GetQuotedTableName(tableName), _dialectProvider.GetQuotedColumnName(fieldName), string.IsNullOrEmpty(alias) ? string.Empty : string.Format(" AS {0}", _dialectProvider.GetQuotedColumnName(alias))));
				else
					lst.Add(string.Format("{0}{1}", _dialectProvider.GetQuotedColumnName(fieldName), string.IsNullOrEmpty(alias) ? string.Empty : string.Format(" AS {0}", _dialectProvider.GetQuotedColumnName(alias))));
				return;
			}
			throw new OrmException("Only Members are allowed");
		}

		/// <summary>Process the new.</summary>
		/// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="tableName">      Name of the table.</param>
		/// <param name="nex">            The nex.</param>
		/// <param name="lst">            The list.</param>
		/// <param name="withTablePrefix">true to with table prefix.</param>
		private void ProcessNew<T>(string tableName, NewExpression nex, List<string> lst, bool withTablePrefix)
		{
			if (nex.Arguments == null || nex.Arguments.Count == 0)
				throw new OrmException("Only column list allowed");

			var expressionProperties = nex.Type.GetProperties();
			for (int i = 0; i < nex.Arguments.Count; i++)
			{
				var arg = nex.Arguments[i];
				var alias = expressionProperties[i].Name;

				PropertyList<T>(tableName, arg, lst, withTablePrefix, alias);
			}
			return;
		}

		/// <summary>Property list.</summary>
		/// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="tableName">      Name of the table.</param>
		/// <param name="exp">            The exponent.</param>
		/// <param name="lst">            The list.</param>
		/// <param name="withTablePrefix">true to with table prefix.</param>
		/// <param name="alias">          The alias.</param>
		private void PropertyList<T>(string tableName, Expression exp, List<string> lst, bool withTablePrefix, string alias = "")
		{
			if (exp == null)
				return;

			switch (exp.NodeType)
			{
				case ExpressionType.MemberAccess:
					ProcessMemberAccess<T>(tableName, exp as MemberExpression, lst, withTablePrefix, alias);
					return;

				case ExpressionType.Convert:
					var ue = exp as UnaryExpression;
					ProcessUnary<T>(tableName, ue, lst, withTablePrefix);
					return;

				case ExpressionType.New:
					ProcessNew<T>(tableName, exp as NewExpression, lst, withTablePrefix);
					return;
			}
			throw new OrmException("Only columns are allowed");
		}

		public JoinSqlBuilder<TNewPoco, TBasePoco> SelectCountDistinct<T>(Expression<Func<T, object>> selectColumn)
		{
			return SelectGenericAggregate<T>(selectColumn, "COUNT", true);
		}

		/// <summary>Selects the given select columns.</summary>
		/// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="selectColumns">The select columns.</param>
		/// <returns>A JoinSqlBuilder&lt;TNewPoco,TBasePoco&gt;</returns>
		public JoinSqlBuilder<TNewPoco, TBasePoco> Select<T>(Expression<Func<T, object>> selectColumns)
		{
			Type associatedType = this.PreviousAssociatedType(typeof(T), typeof(T));
			if (associatedType == null)
			{
				throw new OrmException("Either the source or destination table should be associated ");
			}

			this.columnList.AddRange(ColumnList(associatedType.GetModelDefinition().ModelName, selectColumns));
			return this;
		}

		/// <summary>Select all.</summary>
		/// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <returns>A JoinSqlBuilder&lt;TNewPoco,TBasePoco&gt;</returns>
		public JoinSqlBuilder<TNewPoco, TBasePoco> SelectAll<T>()
		{
			Type associatedType = this.PreviousAssociatedType(typeof(T), typeof(T));
			if (associatedType == null)
			{
				throw new OrmException("Either the source or destination table should be associated ");
			}
			this.columnList.AddRange(ColumnList<T>());
			return this;
		}

		/// <summary>Select distinct.</summary>
		/// <returns>A JoinSqlBuilder&lt;TNewPoco,TBasePoco&gt;</returns>
		public JoinSqlBuilder<TNewPoco, TBasePoco> SelectDistinct()
		{
			isDistinct = true;
			return this;
		}

		/// <summary>Select maximum.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="selectColumn">The select column.</param>
		/// <returns>A JoinSqlBuilder&lt;TNewPoco,TBasePoco&gt;</returns>
		public JoinSqlBuilder<TNewPoco, TBasePoco> SelectMax<T>(Expression<Func<T, object>> selectColumn)
		{
			return SelectGenericAggregate<T>(selectColumn, "MAX");
		}

		/// <summary>Select minimum.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="selectColumn">The select column.</param>
		/// <returns>A JoinSqlBuilder&lt;TNewPoco,TBasePoco&gt;</returns>
		public JoinSqlBuilder<TNewPoco, TBasePoco> SelectMin<T>(Expression<Func<T, object>> selectColumn)
		{
			return SelectGenericAggregate<T>(selectColumn, "MIN");
		}

		/// <summary>Select count.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="selectColumn">The select column.</param>
		/// <returns>A JoinSqlBuilder&lt;TNewPoco,TBasePoco&gt;</returns>
		public JoinSqlBuilder<TNewPoco, TBasePoco> SelectCount<T>(Expression<Func<T, object>> selectColumn)
		{
			return SelectGenericAggregate<T>(selectColumn, "COUNT");
		}

		/// <summary>Select average.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="selectColumn">The select column.</param>
		/// <returns>A JoinSqlBuilder&lt;TNewPoco,TBasePoco&gt;</returns>
		public JoinSqlBuilder<TNewPoco, TBasePoco> SelectAverage<T>(Expression<Func<T, object>> selectColumn)
		{
			return SelectGenericAggregate<T>(selectColumn, "AVG");
		}

		/// <summary>Select sum.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="selectColumn">The select column.</param>
		/// <returns>A JoinSqlBuilder&lt;TNewPoco,TBasePoco&gt;</returns>
		public JoinSqlBuilder<TNewPoco, TBasePoco> SelectSum<T>(Expression<Func<T, object>> selectColumn)
		{
			return SelectGenericAggregate<T>(selectColumn, "SUM");
		}

		/// <summary>Select generic aggregate.</summary>
		/// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="selectColumn">The select column.</param>
		/// <param name="functionName">Name of the function.</param>
		/// <returns>A JoinSqlBuilder&lt;TNewPoco,TBasePoco&gt;</returns>
		private JoinSqlBuilder<TNewPoco, TBasePoco> SelectGenericAggregate<T>(Expression<Func<T, object>> selectColumn, string functionName, bool distinct = false)
		{
			Type associatedType = this.PreviousAssociatedType(typeof(T), typeof(T));
			if (associatedType == null)
			{
				throw new OrmException("Either the source or destination table should be associated ");
			}
			isAggregateUsed = true;

			CheckAggregateUsage(true);

			var columns = ColumnList(associatedType.GetModelDefinition().ModelName, selectColumn);
			if ((columns.Count == 0) || (columns.Count > 1))
			{
				throw new OrmException("Expression should select only one Column ");
			}
			this.columnList.Add(string.Format(" {0}({1}{2}) ", functionName.ToUpper(), distinct ? "DISTINCT " : string.Empty, columns[0]));
			return this;
		}

		/// <summary>Select minimum.</summary>
		/// <returns>A JoinSqlBuilder&lt;TNewPoco,TBasePoco&gt;</returns>
		public JoinSqlBuilder<TNewPoco, TBasePoco> SelectMin()
		{
			isDistinct = true;
			return this;
		}

		/// <summary>Wheres the given where.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="where">The where.</param>
		/// <returns>A JoinSqlBuilder&lt;TNewPoco,TBasePoco&gt;</returns>
		public JoinSqlBuilder<TNewPoco, TBasePoco> Where<T>(Expression<Func<T, bool>> where)
		{
			return WhereInternal(WhereType.AND, where);
		}

		/// <summary>Ors the given where.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="where">The where.</param>
		/// <returns>A JoinSqlBuilder&lt;TNewPoco,TBasePoco&gt;</returns>
		public JoinSqlBuilder<TNewPoco, TBasePoco> Or<T>(Expression<Func<T, bool>> where)
		{
			return WhereInternal(WhereType.OR, where);
		}

		/// <summary>Ands the given where.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="where">The where.</param>
		/// <returns>A JoinSqlBuilder&lt;TNewPoco,TBasePoco&gt;</returns>
		public JoinSqlBuilder<TNewPoco, TBasePoco> And<T>(Expression<Func<T, bool>> where)
		{
			return WhereInternal(WhereType.AND, where);
		}

		/// <summary>Where internal.</summary>
		/// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="whereType">Type of the where.</param>
		/// <param name="where">    The where.</param>
		/// <returns>A JoinSqlBuilder&lt;TNewPoco,TBasePoco&gt;</returns>
		private JoinSqlBuilder<TNewPoco, TBasePoco> WhereInternal<T>(WhereType whereType, Expression<Func<T, bool>> where)
		{
			Type associatedType = this.PreviousAssociatedType(typeof(T), typeof(T));
			if (associatedType == null)
			{
				throw new OrmException("Either the source or destination table should be associated ");
			}
			var ev = _dialectProvider.ExpressionVisitor<T>();
			ev.WhereStatementWithoutWhereString = true;
			ev.PrefixFieldWithTableName = true;
			ev.Where(where);
			var str = ev.WhereExpression;
			if (String.IsNullOrEmpty(str) == false)
			{
				this.whereList.Add(new KeyValuePair<string, WhereType>(str, whereType));
			}
			return this;
		}

		/// <summary>Order by internal.</summary>
		/// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="byDesc">        true to by description.</param>
		/// <param name="orderByColumns">The order by columns.</param>
		/// <returns>A JoinSqlBuilder&lt;TNewPoco,TBasePoco&gt;</returns>
		private JoinSqlBuilder<TNewPoco, TBasePoco> OrderByInternal<T>(bool byDesc, Expression<Func<T, object>> orderByColumns)
		{
			Type associatedType = this.PreviousAssociatedType(typeof(T), typeof(T));
			if (associatedType == null)
			{
				throw new OrmException("Either the source or destination table should be associated ");
			}

			var lst = ColumnList(associatedType.GetModelDefinition().ModelName, orderByColumns);
			foreach (var item in lst)
				orderByList.Add(new KeyValuePair<string, bool>(item, !byDesc));
			return this;
		}

		/// <summary>Clears this object to its blank/initial state.</summary>
		/// <returns>A JoinSqlBuilder&lt;TNewPoco,TBasePoco&gt;</returns>
		public JoinSqlBuilder<TNewPoco, TBasePoco> Clear()
		{
			joinList.Clear();
			whereList.Clear();
			columnList.Clear();
			orderByList.Clear();
			return this;
		}

		/// <summary>Order by.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="sourceColumn">Source column.</param>
		/// <returns>A JoinSqlBuilder&lt;TNewPoco,TBasePoco&gt;</returns>
		public JoinSqlBuilder<TNewPoco, TBasePoco> OrderBy<T>(Expression<Func<T, object>> sourceColumn)
		{
			return OrderByInternal<T>(false, sourceColumn);
		}

		/// <summary>Order by descending.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="sourceColumn">Source column.</param>
		/// <returns>A JoinSqlBuilder&lt;TNewPoco,TBasePoco&gt;</returns>
		public JoinSqlBuilder<TNewPoco, TBasePoco> OrderByDescending<T>(Expression<Func<T, object>> sourceColumn)
		{
			return OrderByInternal<T>(true, sourceColumn);
		}

		/// <summary>Joins.</summary>
		/// <typeparam name="TSourceTable">     Type of the source table.</typeparam>
		/// <typeparam name="TDestinationTable">Type of the destination table.</typeparam>
		/// <param name="sourceColumn">                   Source column.</param>
		/// <param name="destinationColumn">              Destination column.</param>
		/// <param name="sourceTableColumnSelection">     Source table column selection.</param>
		/// <param name="destinationTableColumnSelection">Destination table column selection.</param>
		/// <param name="sourceWhere">                    Source where.</param>
		/// <param name="destinationWhere">               Destination where.</param>
		/// <returns>A JoinSqlBuilder&lt;TNewPoco,TBasePoco&gt;</returns>
		public JoinSqlBuilder<TNewPoco, TBasePoco> Join<TSourceTable, TDestinationTable>(Expression<Func<TSourceTable, object>> sourceColumn, Expression<Func<TDestinationTable, object>> destinationColumn, Expression<Func<TSourceTable, object>> sourceTableColumnSelection = null, Expression<Func<TDestinationTable, object>> destinationTableColumnSelection = null, Expression<Func<TSourceTable, bool>> sourceWhere = null, Expression<Func<TDestinationTable, bool>> destinationWhere = null)
		{
			return JoinInternal<Join, TSourceTable, TDestinationTable>(JoinType.INNER, joinList, sourceColumn, destinationColumn, sourceTableColumnSelection, destinationTableColumnSelection, sourceWhere, destinationWhere);
		}

		/// <summary>Left join.</summary>
		/// <typeparam name="TSourceTable">     Type of the source table.</typeparam>
		/// <typeparam name="TDestinationTable">Type of the destination table.</typeparam>
		/// <param name="sourceColumn">                   Source column.</param>
		/// <param name="destinationColumn">              Destination column.</param>
		/// <param name="sourceTableColumnSelection">     Source table column selection.</param>
		/// <param name="destinationTableColumnSelection">Destination table column selection.</param>
		/// <param name="sourceWhere">                    Source where.</param>
		/// <param name="destinationWhere">               Destination where.</param>
		/// <returns>A JoinSqlBuilder&lt;TNewPoco,TBasePoco&gt;</returns>
		public JoinSqlBuilder<TNewPoco, TBasePoco> LeftJoin<TSourceTable, TDestinationTable>(Expression<Func<TSourceTable, object>> sourceColumn, Expression<Func<TDestinationTable, object>> destinationColumn, Expression<Func<TSourceTable, object>> sourceTableColumnSelection = null, Expression<Func<TDestinationTable, object>> destinationTableColumnSelection = null, Expression<Func<TSourceTable, bool>> sourceWhere = null, Expression<Func<TDestinationTable, bool>> destinationWhere = null)
		{
			return JoinInternal<Join, TSourceTable, TDestinationTable>(JoinType.LEFTOUTER, joinList, sourceColumn, destinationColumn, sourceTableColumnSelection, destinationTableColumnSelection, sourceWhere, destinationWhere);
		}

		/// <summary>Right join.</summary>
		/// <typeparam name="TSourceTable">     Type of the source table.</typeparam>
		/// <typeparam name="TDestinationTable">Type of the destination table.</typeparam>
		/// <param name="sourceColumn">                   Source column.</param>
		/// <param name="destinationColumn">              Destination column.</param>
		/// <param name="sourceTableColumnSelection">     Source table column selection.</param>
		/// <param name="destinationTableColumnSelection">Destination table column selection.</param>
		/// <param name="sourceWhere">                    Source where.</param>
		/// <param name="destinationWhere">               Destination where.</param>
		/// <returns>A JoinSqlBuilder&lt;TNewPoco,TBasePoco&gt;</returns>
		public JoinSqlBuilder<TNewPoco, TBasePoco> RightJoin<TSourceTable, TDestinationTable>(Expression<Func<TSourceTable, object>> sourceColumn, Expression<Func<TDestinationTable, object>> destinationColumn, Expression<Func<TSourceTable, object>> sourceTableColumnSelection = null, Expression<Func<TDestinationTable, object>> destinationTableColumnSelection = null, Expression<Func<TSourceTable, bool>> sourceWhere = null, Expression<Func<TDestinationTable, bool>> destinationWhere = null)
		{
			return JoinInternal<Join, TSourceTable, TDestinationTable>(JoinType.RIGHTOUTER, joinList, sourceColumn, destinationColumn, sourceTableColumnSelection, destinationTableColumnSelection, sourceWhere, destinationWhere);
		}

		/// <summary>Full join.</summary>
		/// <typeparam name="TSourceTable">     Type of the source table.</typeparam>
		/// <typeparam name="TDestinationTable">Type of the destination table.</typeparam>
		/// <param name="sourceColumn">                   Source column.</param>
		/// <param name="destinationColumn">              Destination column.</param>
		/// <param name="sourceTableColumnSelection">     Source table column selection.</param>
		/// <param name="destinationTableColumnSelection">Destination table column selection.</param>
		/// <param name="sourceWhere">                    Source where.</param>
		/// <param name="destinationWhere">               Destination where.</param>
		/// <returns>A JoinSqlBuilder&lt;TNewPoco,TBasePoco&gt;</returns>
		public JoinSqlBuilder<TNewPoco, TBasePoco> FullJoin<TSourceTable, TDestinationTable>(Expression<Func<TSourceTable, object>> sourceColumn, Expression<Func<TDestinationTable, object>> destinationColumn, Expression<Func<TSourceTable, object>> sourceTableColumnSelection = null, Expression<Func<TDestinationTable, object>> destinationTableColumnSelection = null, Expression<Func<TSourceTable, bool>> sourceWhere = null, Expression<Func<TDestinationTable, bool>> destinationWhere = null)
		{
			return JoinInternal<Join, TSourceTable, TDestinationTable>(JoinType.FULLOUTER, joinList, sourceColumn, destinationColumn, sourceTableColumnSelection, destinationTableColumnSelection, sourceWhere, destinationWhere);
		}

		//Not ready yet
		//public JoinSqlBuilder<TNewPoco, TBasePoco> SelfJoin<TSourceTable>(Expression<Func<TSourceTable, object>> sourceColumn, Expression<Func<TSourceTable, object>> destinationColumn, Expression<Func<TSourceTable, object>> sourceTableColumnSelection = null, Expression<Func<TSourceTable, object>> destinationTableColumnSelection = null, Expression<Func<TSourceTable, bool>> sourceWhere = null, Expression<Func<TSourceTable, bool>> destinationWhere = null)
		//{
		//    return JoinInternal<Join, TSourceTable, TSourceTable>(JoinType.SELF, joinList, sourceColumn, destinationColumn, sourceTableColumnSelection, destinationTableColumnSelection, sourceWhere, destinationWhere);
		//}

		/// <summary>Cross join.</summary>
		/// <typeparam name="TSourceTable">     Type of the source table.</typeparam>
		/// <typeparam name="TDestinationTable">Type of the destination table.</typeparam>
		/// <param name="sourceTableColumnSelection">     Source table column selection.</param>
		/// <param name="destinationTableColumnSelection">Destination table column selection.</param>
		/// <param name="sourceWhere">                    Source where.</param>
		/// <param name="destinationWhere">               Destination where.</param>
		/// <returns>A JoinSqlBuilder&lt;TNewPoco,TBasePoco&gt;</returns>
		public JoinSqlBuilder<TNewPoco, TBasePoco> CrossJoin<TSourceTable, TDestinationTable>(Expression<Func<TSourceTable, object>> sourceTableColumnSelection = null, Expression<Func<TDestinationTable, object>> destinationTableColumnSelection = null, Expression<Func<TSourceTable, bool>> sourceWhere = null, Expression<Func<TDestinationTable, bool>> destinationWhere = null)
		{
			return JoinInternal<Join, TSourceTable, TDestinationTable>(JoinType.CROSS, joinList, null, null, sourceTableColumnSelection, destinationTableColumnSelection, sourceWhere, destinationWhere);
		}

		/// <summary>Join internal.</summary>
		/// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
		/// <typeparam name="TJoin">            Type of the join.</typeparam>
		/// <typeparam name="TSourceTable">     Type of the source table.</typeparam>
		/// <typeparam name="TDestinationTable">Type of the destination table.</typeparam>
		/// <param name="joinType">                       Type of the join.</param>
		/// <param name="joinObjList">                    List of join objects.</param>
		/// <param name="sourceColumn">                   Source column.</param>
		/// <param name="destinationColumn">              Destination column.</param>
		/// <param name="sourceTableColumnSelection">     Source table column selection.</param>
		/// <param name="destinationTableColumnSelection">Destination table column selection.</param>
		/// <param name="sourceWhere">                    Source where.</param>
		/// <param name="destinationWhere">               Destination where.</param>
		/// <returns>A JoinSqlBuilder&lt;TNewPoco,TBasePoco&gt;</returns>
		private JoinSqlBuilder<TNewPoco, TBasePoco> JoinInternal<TJoin, TSourceTable, TDestinationTable>(JoinType joinType, List<TJoin> joinObjList, Expression<Func<TSourceTable, object>> sourceColumn, Expression<Func<TDestinationTable, object>> destinationColumn, Expression<Func<TSourceTable, object>> sourceTableColumnSelection, Expression<Func<TDestinationTable, object>> destinationTableColumnSelection, Expression<Func<TSourceTable, bool>> sourceWhere = null, Expression<Func<TDestinationTable, bool>> destinationWhere = null) where TJoin : Join, new()
		{
			Type associatedType = this.PreviousAssociatedType(typeof(TSourceTable), typeof(TDestinationTable));
			if (associatedType == null)
			{
				throw new OrmException("Either the source or destination table should be associated ");
			}

			TJoin join = new TJoin();
			join.JoinType = joinType;
			join.Class1Type = typeof(TSourceTable);
			join.Class2Type = typeof(TDestinationTable);

			if (associatedType == join.Class1Type)
				join.RefType = join.Class2Type;
			else
				join.RefType = join.Class1Type;

			join.Class1Schema = GetSchema(join.Class1Type);
			join.Class1TableName = join.Class1Type.GetModelDefinition().ModelName;
			join.Class2Schema = GetSchema(join.Class2Type);
			join.Class2TableName = join.Class2Type.GetModelDefinition().ModelName;
			join.RefTypeSchema = GetSchema(join.RefType);
			join.RefTypeTableName = join.RefType.GetModelDefinition().ModelName;

			if (join.JoinType != JoinType.CROSS)
			{
				if (join.JoinType == JoinType.SELF)
				{
					join.Class1ColumnName = Column<TSourceTable>(join.Class1TableName, sourceColumn, false);
					join.Class2ColumnName = Column<TDestinationTable>(join.Class2TableName, destinationColumn, false);
				}
				else
				{
					join.Class1ColumnName = Column<TSourceTable>(join.Class1TableName, sourceColumn, true);
					join.Class2ColumnName = Column<TDestinationTable>(join.Class2TableName, destinationColumn, true);
				}
			}

			if (sourceTableColumnSelection != null)
			{
				columnList.AddRange(ColumnList<TSourceTable>(join.Class1TableName, sourceTableColumnSelection));
			}

			if (destinationTableColumnSelection != null)
			{
				columnList.AddRange(ColumnList<TDestinationTable>(join.Class2TableName, destinationTableColumnSelection));
			}

			
			if (sourceWhere != null)
			{
				var ev = _dialectProvider.ExpressionVisitor<TSourceTable>();
				ev.CopyExistingParameters(_parameters);
				ev.WhereStatementWithoutWhereString = true;
				ev.PrefixFieldWithTableName = true;
				ev.Where(sourceWhere);

				_parameters = ev.Parameters;

				var where = ev.WhereExpression;
				if (!String.IsNullOrEmpty(where))
					whereList.Add(new KeyValuePair<string, WhereType>(where, WhereType.AND));
			}

			if (destinationWhere != null)
			{
				var ev = _dialectProvider.ExpressionVisitor<TDestinationTable>();
				ev.CopyExistingParameters(_parameters);
				ev.WhereStatementWithoutWhereString = true;
				ev.PrefixFieldWithTableName = true;
				ev.Where(destinationWhere);

				_parameters = ev.Parameters;

				var where = ev.WhereExpression;
				if (!String.IsNullOrEmpty(where))
					whereList.Add(new KeyValuePair<string, WhereType>(where, WhereType.AND));
			}

			joinObjList.Add(join);
			return this;
		}

		/// <summary>Gets a schema.</summary>
		/// <param name="type">The type.</param>
		/// <returns>The schema.</returns>
		private string GetSchema(Type type)
		{
			return string.IsNullOrEmpty(type.GetModelDefinition().Schema) ? string.Empty : string.Format("\"{0}\".", type.GetModelDefinition().Schema);
		}

		/// <summary>Previous associated type.</summary>
		/// <param name="sourceTableType">     Type of the source table.</param>
		/// <param name="destinationTableType">Type of the destination table.</param>
		/// <returns>A Type.</returns>
		private Type PreviousAssociatedType(Type sourceTableType, Type destinationTableType)
		{
			if (sourceTableType == basePocoType || destinationTableType == basePocoType)
			{
				return basePocoType;
			}

			foreach (var j in joinList)
			{
				if (j.Class1Type == sourceTableType || j.Class2Type == sourceTableType)
				{
					return sourceTableType;
				}
				if (j.Class1Type == destinationTableType || j.Class2Type == destinationTableType)
				{
					return destinationTableType;
				}
			}
			return null;
		}

		/// <summary>Check aggregate usage.</summary>
		/// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
		/// <param name="ignoreCurrentItem">true to ignore current item.</param>
		private void CheckAggregateUsage(bool ignoreCurrentItem)
		{
			if ((columnList.Count > (ignoreCurrentItem ? 0 : 1)) && (isAggregateUsed == true))
			{
				throw new OrmException("Aggregate function cannot be used with non aggregate select columns");
			}
		}

		/// <summary>Converts this object to a SQL.</summary>
		/// <returns>This object as a string.</returns>
		public string ToSql()
		{
			CheckAggregateUsage(false);

			var sb = new StringBuilder();
			sb.Append("SELECT ");

			var colSB = new StringBuilder();

			if (columnList.Count > 0)
			{
				if (isDistinct)
					sb.Append(" DISTINCT ");

				foreach (var col in columnList)
				{
					colSB.AppendFormat("{0}{1}", colSB.Length > 0 ? "," : "", col);
				}
			}
			else
			{
				// improve performance avoiding multiple calls to GetModelDefinition()
				var modelDef = typeof(TNewPoco).GetModelDefinition();

				if (isDistinct && modelDef.FieldDefinitions.Count > 0)
					sb.Append(" DISTINCT ");

				foreach (var fi in modelDef.FieldDefinitions)
				{
					colSB.AppendFormat("{0}{1}", colSB.Length > 0 ? "," : "", (String.IsNullOrEmpty(fi.BelongToModelName) ? (_dialectProvider.GetQuotedTableName(modelDef.ModelName)) : (_dialectProvider.GetQuotedTableName(fi.BelongToModelName))) + "." + _dialectProvider.GetQuotedColumnName(fi.FieldName));
				}
				if (colSB.Length == 0)
					colSB.AppendFormat("\"{0}{1}\".*", baseSchema, _dialectProvider.GetQuotedTableName(baseTableName));
			}

			sb.Append(colSB.ToString() + " \n");

			sb.AppendFormat("FROM {0}{1} \n", baseSchema, _dialectProvider.GetQuotedTableName(baseTableName));
			int i = 0;
			foreach (var join in joinList)
			{
				i++;
				if ((join.JoinType == JoinType.INNER) || (join.JoinType == JoinType.SELF))
					sb.Append(" INNER JOIN ");
				else if (join.JoinType == JoinType.LEFTOUTER)
					sb.Append(" LEFT OUTER JOIN ");
				else if (join.JoinType == JoinType.RIGHTOUTER)
					sb.Append(" RIGHT OUTER JOIN ");
				else if (join.JoinType == JoinType.FULLOUTER)
					sb.Append(" FULL OUTER JOIN ");
				else if (join.JoinType == JoinType.CROSS)
				{
					sb.Append(" CROSS JOIN ");
				}

				if (join.JoinType == JoinType.CROSS)
				{
					sb.AppendFormat(" {0}{1} ON {2} = {3}  \n", join.RefTypeSchema, _dialectProvider.GetQuotedTableName(join.RefTypeTableName));
				}
				else
				{
					if (join.JoinType != JoinType.SELF)
					{
						sb.AppendFormat(" {0}{1} ON {2} = {3}  \n", join.RefTypeSchema, _dialectProvider.GetQuotedTableName(join.RefTypeTableName), join.Class1ColumnName, join.Class2ColumnName);
					}
					else
					{
						sb.AppendFormat(" {0}{1} AS {2} ON {2}.{3} = \"{1}\".{4}  \n", join.RefTypeSchema, _dialectProvider.GetQuotedTableName(join.RefTypeTableName), _dialectProvider.GetQuotedTableName(join.RefTypeTableName) + "_" + i.ToString(), join.Class1ColumnName, join.Class2ColumnName);
					}
				}
			}

			if (whereList.Count > 0)
			{
				var whereSB = new StringBuilder();
				foreach (var where in whereList)
				{
					whereSB.AppendFormat("{0}{1}", whereSB.Length > 0 ? (where.Value == WhereType.OR ? " OR " : " AND ") : "", where.Key);
				}
				sb.Append("WHERE " + whereSB.ToString() + " \n");
			}

			if (orderByList.Count > 0)
			{
				var orderBySB = new StringBuilder();
				foreach (var ob in orderByList)
				{
					orderBySB.AppendFormat("{0}{1} {2} ", orderBySB.Length > 0 ? "," : "", ob.Key, ob.Value ? "ASC" : "DESC");
				}
				sb.Append("ORDER BY " + orderBySB.ToString() + " \n");
			}

			return sb.ToString();
		}
	}

	/// <summary>Values that represent WhereType.</summary>
	enum WhereType
	{
		/// <summary>An enum constant representing the and option.</summary>
		AND,

		/// <summary>An enum constant representing the or option.</summary>
		OR
	}

	/// <summary>Values that represent JoinType.</summary>
	enum JoinType
	{
		/// <summary>An enum constant representing the inner option.</summary>
		INNER,

		/// <summary>An enum constant representing the leftouter option.</summary>
		LEFTOUTER,

		/// <summary>An enum constant representing the rightouter option.</summary>
		RIGHTOUTER,

		/// <summary>An enum constant representing the fullouter option.</summary>
		FULLOUTER,

		/// <summary>An enum constant representing the cross option.</summary>
		CROSS,

		/// <summary>An enum constant representing the self option.</summary>
		SELF
	}

	/// <summary>A join.</summary>
	class Join
	{
		/// <summary>Gets or sets the type of the class 1.</summary>
		/// <value>The type of the class 1.</value>
		public Type Class1Type { get; set; }

		/// <summary>Gets or sets the type of the class 2.</summary>
		/// <value>The type of the class 2.</value>
		public Type Class2Type { get; set; }

		/// <summary>Gets or sets the type of the reference.</summary>
		/// <value>The type of the reference.</value>
		public Type RefType { get; set; }

		/// <summary>Gets or sets the type of the join.</summary>
		/// <value>The type of the join.</value>
		public JoinType JoinType { get; set; }

		/// <summary>Gets or sets the class 1 schema.</summary>
		/// <value>The class 1 schema.</value>
		public string Class1Schema { get; set; }

		/// <summary>Gets or sets the class 2 schema.</summary>
		/// <value>The class 2 schema.</value>
		public string Class2Schema { get; set; }

		/// <summary>Gets or sets the name of the class 1 table.</summary>
		/// <value>The name of the class 1 table.</value>
		public string Class1TableName { get; set; }

		/// <summary>Gets or sets the name of the class 2 table.</summary>
		/// <value>The name of the class 2 table.</value>
		public string Class2TableName { get; set; }

		/// <summary>Gets or sets the reference type schema.</summary>
		/// <value>The reference type schema.</value>
		public string RefTypeSchema { get; set; }

		/// <summary>Gets or sets the name of the reference type table.</summary>
		/// <value>The name of the reference type table.</value>
		public string RefTypeTableName { get; set; }

		/// <summary>Gets or sets the name of the class 1 column.</summary>
		/// <value>The name of the class 1 column.</value>
		public string Class1ColumnName { get; set; }

		/// <summary>Gets or sets the name of the class 2 column.</summary>
		/// <value>The name of the class 2 column.</value>
		public string Class2ColumnName { get; set; }
	}
}