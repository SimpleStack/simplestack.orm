using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace SimpleStack.Orm.Expressions
{
	/// <summary>A SQL expression visitor.</summary>
	/// <typeparam name="T">Generic type parameter.</typeparam>
	public abstract class SqlExpressionVisitor<T>
	{
		protected IDialectProvider DialectProvider { get; }

		private readonly IDictionary<string, object> _parameters = new Dictionary<string, object>();

		/// <summary>The order by properties.</summary>
		private readonly List<string> _orderByProperties = new List<string>();

		/// <summary>The underlying expression.</summary>
		private Expression<Func<T, bool>> _underlyingExpression;

		/// <summary>Describes who group this object.</summary>
		private string _groupBy = string.Empty;

		/// <summary>The having expression.</summary>
		private string _havingExpression;

		/// <summary>The insert fields.</summary>
		private IList<string> _insertFields = new List<string>();

		/// <summary>The model definition.</summary>
		private ModelDefinition _modelDef;

		/// <summary>Describes who order this object.</summary>
		private string _orderBy = string.Empty;

		/// <summary>Options for controlling the operation.</summary>
		public Dictionary<string, object> Params = new Dictionary<string, object>();

		/// <summary>The select expression.</summary>
		private string _selectExpression = string.Empty;

		/// <summary>The separator.</summary>
		private string _sep = string.Empty;

		/// <summary>The update fields.</summary>
		//private IList<string> _updateFields = new List<string>();

		/// <summary>The fields.</summary>
		private IList<string> _fields = new List<string>();

		/// <summary>true to use field name.</summary>
		private bool _useFieldName;

		/// <summary>The where expression.</summary>
		private string _whereExpression;

		/// <summary>
		///     Initializes a new instance of the NServiceKit.OrmLite.SqlExpressionVisitor&lt;T&gt; class.
		/// </summary>
		protected SqlExpressionVisitor(IDialectProvider dialectProvider)
		{
			DialectProvider = dialectProvider;
			_modelDef = typeof(T).GetModelDefinition();
			PrefixFieldWithTableName = false;
		}

		/// <summary>
		///     Gets or sets a value indicating whether the prefix field with table name.
		/// </summary>
		/// <value>true if prefix field with table name, false if not.</value>
		public bool PrefixFieldWithTableName { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the where statement without where string.
		/// </summary>
		/// <value>true if where statement without where string, false if not.</value>
		public bool WhereStatementWithoutWhereString { get; set; }

		/// <summary>Gets the separator.</summary>
		/// <value>The separator.</value>
		protected string Sep
		{
			get { return _sep; }
		}

		/// <summary>Gets or sets the select expression.</summary>
		/// <value>The select expression.</value>
		public string SelectExpression
		{
			get
			{
				if (string.IsNullOrEmpty(_selectExpression))
					BuildSelectExpression();
				return _selectExpression;
			}
			set { _selectExpression = value; }
		}

		/// <summary>Gets or sets the where expression.</summary>
		/// <value>The where expression.</value>
		public string WhereExpression
		{
			get { return _whereExpression; }
			set { _whereExpression = value; }
		}

		/// <summary>Gets or sets the group by expression.</summary>
		/// <value>The group by expression.</value>
		public string GroupByExpression
		{
			get { return _groupBy; }
			set { _groupBy = value; }
		}

		/// <summary>Gets or sets the having expression.</summary>
		/// <value>The having expression.</value>
		public string HavingExpression
		{
			get { return _havingExpression; }
			set { _havingExpression = value; }
		}

		/// <summary>Gets or sets the order by expression.</summary>
		/// <value>The order by expression.</value>
		public string OrderByExpression
		{
			get { return _orderBy; }
			set { _orderBy = value; }
		}

		/// <summary>Gets the limit expression.</summary>
		/// <value>The limit expression.</value>
		public virtual string LimitExpression
		{
			get
			{
				return DialectProvider.GetLimitExpression(Skip, Rows);
			}
		}

		/// <summary>
		/// Get the maximum number of rows to return. Use Limit() to change the value
		/// </summary>
		public int? Rows { get; internal set; }

		/// <summary>
		/// Get the number of rows skipped. Use Limit() to change the value
		/// </summary>
		public int? Skip { get; internal set; }

		public bool IsDistinct { get; private set; }

		/// <summary>Gets or sets the insert fields.</summary>
		/// <value>The insert fields.</value>
		public IList<string> InsertFields
		{
			get { return _insertFields; }
			set { _insertFields = value; }
		}

		//Query fields
		public IList<string> Fields
		{
			get { return _fields; }
		}

		/// <summary>Gets or sets the model definition.</summary>
		/// <value>The model definition.</value>
		public ModelDefinition ModelDefinition
		{
			get { return _modelDef; }
			set { _modelDef = value; }
		}

		public IDictionary<string, object> Parameters
		{
			get { return _parameters; }
		}

		internal void CopyExistingParameters(IDictionary<string, object> parameters)
		{
			if (parameters == null)
				return;

			foreach(var p in parameters)
				_parameters.Add(p);
		}

		public virtual SqlExpressionVisitor<T> Distinct(bool distinct = true)
		{
			IsDistinct = distinct;
			return this;
		}

		///// <summary>Clear select expression. All properties will be selected.</summary>
		///// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		//public virtual SqlExpressionVisitor<T> Select()
		//{
		//	return Select(string.Empty);
		//}

		///// <summary>set the specified selectExpression.</summary>
		///// <param name="selectExpression">
		/////     raw Select expression: "Select SomeField1, SomeField2 from
		/////     SomeTable".
		///// </param>
		///// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		//public virtual SqlExpressionVisitor<T> Select(string selectExpression)
		//{
		//	if (string.IsNullOrEmpty(selectExpression))
		//	{
		//		BuildSelectExpression(string.Empty);
		//	}
		//	else
		//	{
		//		this._selectExpression = selectExpression;
		//	}
		//	return this;
		//}

		/// <summary>Fields to be selected.</summary>
		/// <typeparam name="TKey">objectWithProperties.</typeparam>
		/// <param name="fields">x=> x.SomeProperty1 or x=> new{ x.SomeProperty1, x.SomeProperty2}</param>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		public virtual SqlExpressionVisitor<T> Select<TKey>(Expression<Func<T, TKey>> fields)
		{
			_sep = string.Empty;
			_useFieldName = true;
			_fields = Visit(fields).ToString().Split(',').ToList();
			return this;
		}

		/// <summary>Select distinct.</summary>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		/// <param name="fields">x=> x.SomeProperty1 or x=> new{ x.SomeProperty1, x.SomeProperty2}</param>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		public virtual SqlExpressionVisitor<T> SelectDistinct<TKey>(Expression<Func<T, TKey>> fields)
		{
			_sep = string.Empty;
			_useFieldName = true;
			_fields = Visit(fields).ToString().Split(',').ToList();
			IsDistinct = true;
			return this;
		}

		public SqlExpressionVisitor<T> Clear()
		{
			_underlyingExpression = null;
			_fields.Clear();
			IsDistinct = false;
			return this;
		}

		/// <summary>Wheres the given predicate.</summary>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		//public virtual SqlExpressionVisitor<T> Where()
		//{
		//	if (_underlyingExpression != null)
		//		_underlyingExpression = null; //Where() clears the expression
		//	return Where(string.Empty);
		//}

		///// <summary>Wheres the given predicate.</summary>
		///// <param name="sqlFilter">   A filter specifying the SQL.</param>
		///// <param name="filterParams">Options for controlling the filter.</param>
		///// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		//public virtual SqlExpressionVisitor<T> Where(string sqlFilter, params object[] filterParams)
		//{
		//	_whereExpression = !string.IsNullOrEmpty(sqlFilter) ? sqlFilter.SqlFormat(filterParams) : string.Empty;
		//	if (!string.IsNullOrEmpty(_whereExpression))
		//		_whereExpression = (WhereStatementWithoutWhereString ? "" : "WHERE ") + _whereExpression;
		//	return this;
		//}

		/// <summary>Wheres the given predicate.</summary>
		/// <param name="predicate">The predicate.</param>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		public virtual SqlExpressionVisitor<T> Where(Expression<Func<T, bool>> predicate)
		{
			if (predicate != null)
			{
				And(predicate);
			}
			else
			{
				_underlyingExpression = null;
				_whereExpression = string.Empty;
			}

			return this;
		}

		/// <summary>Ands the given predicate.</summary>
		/// <param name="predicate">The predicate.</param>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		public virtual SqlExpressionVisitor<T> And(Expression<Func<T, bool>> predicate)
		{
			if (predicate != null)
			{
				_underlyingExpression = _underlyingExpression == null ? predicate : _underlyingExpression.And(predicate);

				ProcessInternalExpression();
			}
			return this;
		}

		/// <summary>Ors the given predicate.</summary>
		/// <param name="predicate">The predicate.</param>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		public virtual SqlExpressionVisitor<T> Or(Expression<Func<T, bool>> predicate)
		{
			if (predicate != null)
			{
				_underlyingExpression = _underlyingExpression == null ? predicate : _underlyingExpression.Or(predicate);

				ProcessInternalExpression();
			}
			return this;
		}

		/// <summary>Process the internal expression.</summary>
		private void ProcessInternalExpression()
		{
			_useFieldName = true;
			_sep = " ";
			_whereExpression = Visit(_underlyingExpression).ToString();
			if (!string.IsNullOrEmpty(_whereExpression))
			{
				_whereExpression = (WhereStatementWithoutWhereString ? String.Empty : "WHERE ") + _whereExpression;
			}
		}

		/// <summary>Group by.</summary>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		public virtual SqlExpressionVisitor<T> GroupBy()
		{
			return GroupBy(string.Empty);
		}

		/// <summary>Group by.</summary>
		/// <param name="groupBy">Describes who group this object.</param>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		public virtual SqlExpressionVisitor<T> GroupBy(string groupBy)
		{
			_groupBy = groupBy;
			return this;
		}

		/// <summary>Group by.</summary>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		/// <param name="keySelector">The key selector.</param>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		public virtual SqlExpressionVisitor<T> GroupBy<TKey>(Expression<Func<T, TKey>> keySelector)
		{
			_sep = string.Empty;
			_useFieldName = true;
			_groupBy = Visit(keySelector).ToString();
			if (!string.IsNullOrEmpty(_groupBy)) _groupBy = string.Format(" GROUP BY {0}", _groupBy);
			return this;
		}

		/// <summary>Havings the given predicate.</summary>
		/// <param name="predicate">The predicate.</param>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		public virtual SqlExpressionVisitor<T> Having(Expression<Func<T, bool>> predicate)
		{
			if (predicate != null)
			{
				_useFieldName = true;
				_sep = " ";
				_havingExpression = Visit(predicate).ToString();
				if (!string.IsNullOrEmpty(_havingExpression)) _havingExpression = "HAVING " + _havingExpression;
			}
			else
				_havingExpression = string.Empty;

			return this;
		}

		/// <summary>Order by.</summary>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		public virtual SqlExpressionVisitor<T> OrderBy()
		{
			return OrderBy(string.Empty);
		}

		/// <summary>Order by.</summary>
		/// <param name="orderBy">Describes who order this object.</param>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		public virtual SqlExpressionVisitor<T> OrderBy(string orderBy)
		{
			_orderByProperties.Clear();
			_orderBy = orderBy;
			return this;
		}

		/// <summary>Order by.</summary>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		/// <param name="keySelector">The key selector.</param>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		public virtual SqlExpressionVisitor<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
		{
			_sep = string.Empty;
			_useFieldName = true;
			_orderByProperties.Clear();
			var property = Visit(keySelector).ToString();
			_orderByProperties.Add(property + " ASC");
			BuildOrderByClauseInternal();
			return this;
		}

		/// <summary>Then by.</summary>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		/// <param name="keySelector">The key selector.</param>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		public virtual SqlExpressionVisitor<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
		{
			_sep = string.Empty;
			_useFieldName = true;
			var property = Visit(keySelector).ToString();
			_orderByProperties.Add(property + " ASC");
			BuildOrderByClauseInternal();
			return this;
		}

		/// <summary>Order by descending.</summary>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		/// <param name="keySelector">The key selector.</param>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		public virtual SqlExpressionVisitor<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
		{
			_sep = string.Empty;
			_useFieldName = true;
			_orderByProperties.Clear();
			var property = Visit(keySelector).ToString();
			_orderByProperties.Add(property + " DESC");
			BuildOrderByClauseInternal();
			return this;
		}

		/// <summary>Then by descending.</summary>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		/// <param name="keySelector">The key selector.</param>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		public virtual SqlExpressionVisitor<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
		{
			_sep = string.Empty;
			_useFieldName = true;
			var property = Visit(keySelector).ToString();
			_orderByProperties.Add(property + " DESC");
			BuildOrderByClauseInternal();
			return this;
		}

		/// <summary>Builds order by clause internal.</summary>
		private void BuildOrderByClauseInternal()
		{
			if (_orderByProperties.Count > 0)
			{
				_orderBy = "ORDER BY ";
				foreach (var prop in _orderByProperties)
				{
					_orderBy += prop + ",";
				}
				_orderBy = _orderBy.TrimEnd(',');
			}
			else
			{
				_orderBy = null;
			}
		}

		/// <summary>Set the specified offset and rows for SQL Limit clause.</summary>
		/// <param name="skip">Offset of the first row to return. The offset of the initial row is 0.</param>
		/// <param name="rows">Number of rows returned by a SELECT statement.</param>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		public virtual SqlExpressionVisitor<T> Limit(int skip, int rows)
		{
			Rows = rows;
			Skip = skip;
			return this;
		}

		/// <summary>Set the specified rows for Sql Limit clause.</summary>
		/// <param name="rows">Number of rows returned by a SELECT statement.</param>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		public virtual SqlExpressionVisitor<T> Limit(int rows)
		{
			Rows = rows;
			Skip = 0;
			return this;
		}

		/// <summary>Clear Sql Limit clause.</summary>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		public virtual SqlExpressionVisitor<T> Limit()
		{
			Skip = null;
			Rows = null;
			return this;
		}
		
		/// <summary>Fields to be updated.</summary>
		/// <typeparam name="TKey">objectWithProperties.</typeparam>
		/// <param name="fields">x=> x.SomeProperty1 or x=> new{ x.SomeProperty1, x.SomeProperty2}</param>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		public virtual SqlExpressionVisitor<T> Update<TKey>(Expression<Func<T, TKey>> fields)
		{
			_sep = string.Empty;
			_useFieldName = false;
			_fields = Visit(fields).ToString().Split(',').ToList();
			return this;
		}

		/// <summary>Clear UpdateFields list ( all fields will be updated)</summary>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		//public virtual SqlExpressionVisitor<T> Update()
		//{
		//	_fields = new List<string>();
		//	return this;
		//}

		/// <summary>Fields to be inserted.</summary>
		/// <typeparam name="TKey">objectWithProperties.</typeparam>
		/// <param name="fields">x=> x.SomeProperty1 or x=> new{ x.SomeProperty1, x.SomeProperty2}</param>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		public virtual SqlExpressionVisitor<T> Insert<TKey>(Expression<Func<T, TKey>> fields)
		{
			_sep = string.Empty;
			_useFieldName = false;
			_insertFields = Visit(fields).ToString().Split(',').ToList();
			return this;
		}

		/// <summary>fields to be inserted.</summary>
		/// <param name="insertFields">IList&lt;string&gt; containing Names of properties to be inserted.</param>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		public virtual SqlExpressionVisitor<T> Insert(IList<string> insertFields)
		{
			_insertFields = insertFields;
			return this;
		}

		/// <summary>Clear InsertFields list ( all fields will be inserted)</summary>
		/// <returns>A SqlExpressionVisitor&lt;T&gt;</returns>
		public virtual SqlExpressionVisitor<T> Insert()
		{
			_insertFields = new List<string>();
			return this;
		}

		/// <summary>Visits the given exponent.</summary>
		/// <param name="exp">The exponent.</param>
		/// <returns>An object.</returns>
		protected internal virtual object Visit(Expression exp)
		{
			if (exp == null) return string.Empty;
			switch (exp.NodeType)
			{
				case ExpressionType.Lambda:
					return VisitLambda(exp as LambdaExpression);
				case ExpressionType.MemberAccess:
					return VisitMemberAccess(exp as MemberExpression);
				case ExpressionType.Constant:
					return VisitConstant(exp as ConstantExpression);
				case ExpressionType.Add:
				case ExpressionType.AddChecked:
				case ExpressionType.Subtract:
				case ExpressionType.SubtractChecked:
				case ExpressionType.Multiply:
				case ExpressionType.MultiplyChecked:
				case ExpressionType.Divide:
				case ExpressionType.Modulo:
				case ExpressionType.And:
				case ExpressionType.AndAlso:
				case ExpressionType.Or:
				case ExpressionType.OrElse:
				case ExpressionType.LessThan:
				case ExpressionType.LessThanOrEqual:
				case ExpressionType.GreaterThan:
				case ExpressionType.GreaterThanOrEqual:
				case ExpressionType.Equal:
				case ExpressionType.NotEqual:
				case ExpressionType.Coalesce:
				case ExpressionType.ArrayIndex:
				case ExpressionType.RightShift:
				case ExpressionType.LeftShift:
				case ExpressionType.ExclusiveOr:
					//return "(" + VisitBinary(exp as BinaryExpression) + ")";
					return VisitBinary(exp as BinaryExpression);
				case ExpressionType.Negate:
				case ExpressionType.NegateChecked:
				case ExpressionType.Not:
				case ExpressionType.Convert:
				case ExpressionType.ConvertChecked:
				case ExpressionType.ArrayLength:
				case ExpressionType.Quote:
				case ExpressionType.TypeAs:
					return VisitUnary(exp as UnaryExpression);
				case ExpressionType.Parameter:
					return VisitParameter(exp as ParameterExpression);
				case ExpressionType.Call:
					return VisitMethodCall(exp as MethodCallExpression);
				case ExpressionType.New:
					return VisitNew(exp as NewExpression);
				case ExpressionType.NewArrayInit:
				case ExpressionType.NewArrayBounds:
					return VisitNewArray(exp as NewArrayExpression);
				case ExpressionType.MemberInit:
					return VisitMemberInit(exp as MemberInitExpression);
				default:
					return exp.ToString();
			}
		}

		/// <summary>Visit lambda.</summary>
		/// <param name="lambda">The lambda.</param>
		/// <returns>An object.</returns>
		protected virtual object VisitLambda(LambdaExpression lambda)
		{
			if (lambda.Body.NodeType == ExpressionType.MemberAccess && _sep == " ")
			{
				var m = lambda.Body as MemberExpression;

				if (m?.Expression != null)
				{
					var r = VisitMemberAccess(m).ToString();
					return string.Format("{0}={1}", r, GetQuotedTrueValue());
				}
			}
			return Visit(lambda.Body);
		}

		/// <summary>Visit binary.</summary>
		/// <param name="b">The BinaryExpression to process.</param>
		/// <returns>An object.</returns>
		protected virtual object VisitBinary(BinaryExpression b)
		{
			object left, right;
			var operand = BindOperant(b.NodeType); //sep= " " ??
			if (operand == "AND" || operand == "OR")
			{
				var m = b.Left as MemberExpression;
				if (m != null && m.Expression != null
					&& m.Expression.NodeType == ExpressionType.Parameter)
					left = new PartialSqlString(string.Format("{0}={1}", VisitMemberAccess(m), GetQuotedTrueValue()));
				else
					left = Visit(b.Left);

				m = b.Right as MemberExpression;
				if (m != null && m.Expression != null
					&& m.Expression.NodeType == ExpressionType.Parameter)
					right = new PartialSqlString(string.Format("{0}={1}", VisitMemberAccess(m), GetQuotedTrueValue()));
				else
					right = Visit(b.Right);

				if (!(left is PartialSqlString) && !(right is PartialSqlString))
				{
					var result = Expression.Lambda(b).Compile().DynamicInvoke();
					return new PartialSqlString(AddParameter(result));
				}

				if (!(left is PartialSqlString))
					left = ((bool)left) ? GetTrueExpression() : GetFalseExpression();
				if (!(right is PartialSqlString))
					right = ((bool)right) ? GetTrueExpression() : GetFalseExpression();
			}
			else
			{
				left = Visit(b.Left);
				right = Visit(b.Right);

				if (left is EnumMemberAccess)
				{
					//enum value was may have been returned by Visit(b.Right) as Integer, we have to convert it to Enum
					if (right is Enum)
					{
						right = AddParameter(right);
					}
					else
					{
						right = AddParameter(Enum.ToObject(((EnumMemberAccess)left).EnumType, right));
					}
				}
				else if (right is EnumMemberAccess)
				{
					//enum value was may have been returned by Visit(b.Left) as Integer, we have to convert it to Enum
					if (left is Enum)
					{
						left = AddParameter(left);
					}
					else
					{
						left = AddParameter(Enum.ToObject(((EnumMemberAccess)right).EnumType, left));
					}
				}
				else if (!(left is PartialSqlString) && !(right is PartialSqlString))
				{
					var result = Expression.Lambda(b).Compile().DynamicInvoke();
					return result;
				}
				else if (!(left is PartialSqlString))
				{
					left = AddParameter(left);
				}
				else if (!(right is PartialSqlString))
				{
					right = AddParameter(right);
				}
			}

			if (operand == "=" && right.ToString().Equals("null", StringComparison.InvariantCultureIgnoreCase)) operand = "is";
			else if (operand == "<>" && right.ToString().Equals("null", StringComparison.InvariantCultureIgnoreCase))
				operand = "is not";

			switch (operand)
			{
				case "MOD":
				case "COALESCE":
					return new PartialSqlString(string.Format("{0}({1},{2})", operand, left, right));
				default:
					return new PartialSqlString("(" + left + _sep + operand + _sep + right + ")");
			}
		}

		/// <summary>Visit member access.</summary>
		/// <param name="m">The MethodCallExpression to process.</param>
		/// <returns>An object.</returns>
		protected virtual object VisitMemberAccess(MemberExpression m)
		{
			if (m.Expression != null
				&& (m.Expression.NodeType == ExpressionType.Parameter || m.Expression.NodeType == ExpressionType.Convert))
			{
				var propertyInfo = m.Member as PropertyInfo;

				if (propertyInfo != null && propertyInfo.PropertyType.IsEnum)
					return
						new EnumMemberAccess(
							(PrefixFieldWithTableName ? DialectProvider.GetQuotedTableName(_modelDef.ModelName) + "." : "") +
							GetQuotedColumnName(m.Member.Name), propertyInfo.PropertyType);

				return
					new PartialSqlString((PrefixFieldWithTableName
						? DialectProvider.GetQuotedTableName(_modelDef.ModelName) + "."
						: "") + GetQuotedColumnName(m.Member.Name));
			}

			var member = Expression.Convert(m, typeof(object));
			var lambda = Expression.Lambda<Func<object>>(member);
			var getter = lambda.Compile();
			return getter();
		}

		/// <summary>Visit member initialise.</summary>
		/// <param name="exp">The exponent.</param>
		/// <returns>An object.</returns>
		protected virtual object VisitMemberInit(MemberInitExpression exp)
		{
			return Expression.Lambda(exp).Compile().DynamicInvoke();
		}

		/// <summary>Visit new.</summary>
		/// <param name="nex">The nex.</param>
		/// <returns>An object.</returns>
		protected virtual object VisitNew(NewExpression nex)
		{
			// TODO : check !
			var member = Expression.Convert(nex, typeof(object));
			var lambda = Expression.Lambda<Func<object>>(member);
			try
			{
				var getter = lambda.Compile();
				return getter();
			}
			catch (InvalidOperationException)
			{
				// FieldName ?
				var exprs = VisitExpressionList(nex.Arguments);
				var r = new StringBuilder();
				foreach (var e in exprs)
				{
					r.AppendFormat("{0}{1}",
						r.Length > 0 ? "," : "",
						e);
				}
				return r.ToString();
			}
		}

		/// <summary>Visit parameter.</summary>
		/// <param name="p">The ParameterExpression to process.</param>
		/// <returns>An object.</returns>
		protected virtual object VisitParameter(ParameterExpression p)
		{
			return p.Name;
		}

		/// <summary>Visit constant.</summary>
		/// <param name="c">The ConstantExpression to process.</param>
		/// <returns>An object.</returns>
		protected virtual object VisitConstant(ConstantExpression c)
		{
			if (c.Value == null)
				return new PartialSqlString("null");

			return c.Value;
		}

		/// <summary>Visit unary.</summary>
		/// <param name="u">The UnaryExpression to process.</param>
		/// <returns>An object.</returns>
		protected virtual object VisitUnary(UnaryExpression u)
		{
			switch (u.NodeType)
			{
				case ExpressionType.Not:
					var o = Visit(u.Operand);

					if (!(o is PartialSqlString))
						return !((bool)o);

					if (IsFieldName(o))
						o = o + "=" + GetQuotedTrueValue();

					return new PartialSqlString("NOT (" + o + ")");
				case ExpressionType.Convert:
					if (u.Method != null)
						return Expression.Lambda(u).Compile().DynamicInvoke();
					break;
			}

			return Visit(u.Operand);
		}

		/// <summary>Query if 'm' is column access.</summary>
		/// <param name="m">The MethodCallExpression to process.</param>
		/// <returns>true if column access, false if not.</returns>
		private bool IsColumnAccess(MethodCallExpression m)
		{
			if (m.Object is MethodCallExpression)
				return IsColumnAccess((MethodCallExpression) m.Object);

			var exp = m.Object as MemberExpression;
			return exp?.Expression != null && exp.Expression.Type == typeof(T) && exp.Expression.NodeType == ExpressionType.Parameter;
		}

		/// <summary>Visit method call.</summary>
		/// <param name="m">The MethodCallExpression to process.</param>
		/// <returns>An object.</returns>
		protected virtual object VisitMethodCall(MethodCallExpression m)
		{
			if (m.Method.DeclaringType == typeof(Sql))
				return VisitSqlMethodCall(m);

			if (IsArrayMethod(m))
				return VisitArrayMethodCall(m);

			if (IsColumnAccess(m))
				return VisitColumnAccessMethod(m);

			return Expression.Lambda(m).Compile().DynamicInvoke();
		}

		/// <summary>Visit expression list.</summary>
		/// <param name="original">The original.</param>
		/// <returns>A List&lt;Object&gt;</returns>
		protected virtual List<Object> VisitExpressionList(ReadOnlyCollection<Expression> original)
		{
			var list = new List<Object>();
			for (int i = 0, n = original.Count; i < n; i++)
			{
				if (original[i].NodeType == ExpressionType.NewArrayInit ||
					original[i].NodeType == ExpressionType.NewArrayBounds)
				{
					list.AddRange(VisitNewArrayFromExpressionList(original[i] as NewArrayExpression));
				}
				else
					list.Add(Visit(original[i]));
			}
			return list;
		}

		/// <summary>Visit new array.</summary>
		/// <param name="na">The na.</param>
		/// <returns>An object.</returns>
		protected virtual object VisitNewArray(NewArrayExpression na)
		{
			var exprs = VisitExpressionList(na.Expressions);
			var r = new StringBuilder();
			foreach (var e in exprs)
			{
				r.Append(r.Length > 0 ? "," + e : e);
			}

			return r.ToString();
		}

		/// <summary>Visit new array from expression list.</summary>
		/// <param name="na">The na.</param>
		/// <returns>A List&lt;Object&gt;</returns>
		protected virtual List<Object> VisitNewArrayFromExpressionList(NewArrayExpression na)
		{
			var exprs = VisitExpressionList(na.Expressions);
			return exprs;
		}

		/// <summary>Bind operant.</summary>
		/// <param name="e">The ExpressionType to process.</param>
		/// <returns>A string.</returns>
		protected virtual string BindOperant(ExpressionType e)
		{
			switch (e)
			{
				case ExpressionType.Equal:
					return "=";
				case ExpressionType.NotEqual:
					return "<>";
				case ExpressionType.GreaterThan:
					return ">";
				case ExpressionType.GreaterThanOrEqual:
					return ">=";
				case ExpressionType.LessThan:
					return "<";
				case ExpressionType.LessThanOrEqual:
					return "<=";
				case ExpressionType.AndAlso:
					return "AND";
				case ExpressionType.OrElse:
					return "OR";
				case ExpressionType.Add:
					return "+";
				case ExpressionType.Subtract:
					return "-";
				case ExpressionType.Multiply:
					return "*";
				case ExpressionType.Divide:
					return "/";
				case ExpressionType.Modulo:
					return "MOD";
				case ExpressionType.Coalesce:
					return "COALESCE";
				default:
					return e.ToString();
			}
		}

		/// <summary>Gets quoted column name.</summary>
		/// <param name="memberName">Name of the member.</param>
		/// <returns>The quoted column name.</returns>
		protected virtual string GetQuotedColumnName(string memberName)
		{
			if (_useFieldName)
			{
				var fd = _modelDef.FieldDefinitions.FirstOrDefault(x => x.Name == memberName);
				var fn = fd != default(FieldDefinition) ? fd.FieldName : memberName;
				return DialectProvider.GetQuotedColumnName(fn);
			}
			return memberName;
		}

		/// <summary>Removes the quote from alias described by exp.</summary>
		/// <param name="exp">The exponent.</param>
		/// <returns>A string.</returns>
		protected string RemoveQuoteFromAlias(string exp)
		{
			if ((exp.StartsWith("\"") || exp.StartsWith("`") || exp.StartsWith("'"))
				&&
				(exp.EndsWith("\"") || exp.EndsWith("`") || exp.EndsWith("'")))
			{
				exp = exp.Remove(0, 1);
				exp = exp.Remove(exp.Length - 1, 1);
			}
			return exp;
		}

		/// <summary>Query if 'quotedExp' is field name.</summary>
		/// <param name="quotedExp">The quoted exponent.</param>
		/// <returns>true if field name, false if not.</returns>
		protected bool IsFieldName(object quotedExp)
		{
			var fd =
				_modelDef.FieldDefinitions.
					FirstOrDefault(x =>
						DialectProvider.
							GetQuotedColumnName(x.FieldName) == quotedExp.ToString());
			return (fd != default(FieldDefinition));
		}

		/// <summary>Gets true expression.</summary>
		/// <returns>The true expression.</returns>
		protected object GetTrueExpression()
		{
			return new PartialSqlString(string.Format("({0}={1})", GetQuotedTrueValue(), GetQuotedTrueValue()));
		}

		/// <summary>Gets false expression.</summary>
		/// <returns>The false expression.</returns>
		protected object GetFalseExpression()
		{
			return new PartialSqlString(string.Format("({0}={1})", GetQuotedTrueValue(), GetQuotedFalseValue()));
		}

		/// <summary>Gets quoted true value.</summary>
		/// <returns>The quoted true value.</returns>
		protected object GetQuotedTrueValue()
		{
			return new PartialSqlString(AddParameter(true));
		}

		/// <summary>Gets quoted false value.</summary>
		/// <returns>The quoted false value.</returns>
		protected object GetQuotedFalseValue()
		{
			return new PartialSqlString(AddParameter(false));
		}

		/// <summary>Builds select expression.</summary>
		private void BuildSelectExpression()
		{
			_selectExpression = string.Format("SELECT {0}{1} \nFROM {2}",
				(IsDistinct ? "DISTINCT " : string.Empty),
				(_fields.Count == 0)
					? DialectProvider.GetColumnNames(_modelDef)
					: _fields.Aggregate((x, y) => x + ", " + y),
				DialectProvider.GetQuotedTableName(_modelDef));
		}

		/// <summary>Gets all fields.</summary>
		/// <returns>all fields.</returns>
		public IList<string> GetAllFields()
		{
			return _modelDef.FieldDefinitions.ConvertAll(r => r.Name);
		}

		/// <summary>Applies the paging described by SQL.</summary>
		/// <param name="sql">The SQL.</param>
		/// <returns>A string.</returns>
		protected virtual string ApplyPaging(string sql)
		{
			sql = sql + (string.IsNullOrEmpty(LimitExpression) ? "" : "\n" + LimitExpression);
			return sql;
		}

		/// <summary>Query if 'm' is array method.</summary>
		/// <param name="m">The MethodCallExpression to process.</param>
		/// <returns>true if array method, false if not.</returns>
		private bool IsArrayMethod(MethodCallExpression m)
		{
			if (m.Object == null && m.Method.Name == "Contains")
			{
				if (m.Arguments.Count == 2)
					return true;
			}

			return false;
		}

		/// <summary>Visit array method call.</summary>
		/// <exception cref="NotSupportedException">Thrown when the requested operation is not supported.</exception>
		/// <param name="m">The MethodCallExpression to process.</param>
		/// <returns>An object.</returns>
		protected virtual object VisitArrayMethodCall(MethodCallExpression m)
		{
			string statement;

			switch (m.Method.Name)
			{
				case "Contains":
					var args = VisitExpressionList(m.Arguments);
					var quotedColName = args[1];

					var memberExpr = m.Arguments[0];
					if (memberExpr.NodeType == ExpressionType.MemberAccess)
						memberExpr = (m.Arguments[0] as MemberExpression);

					var member = Expression.Convert(memberExpr, typeof(object));
					var lambda = Expression.Lambda<Func<object>>(member);
					var getter = lambda.Compile();

					var inArgs = Sql.Flatten(getter() as IEnumerable);

					var sIn = new StringBuilder();

					if (inArgs.Any())
					{
						foreach (var e in inArgs)
						{
							sIn.AppendFormat("{0}{1}",
								sIn.Length > 0 ? "," : "",
								AddParameter(e));
						}
					}
					else
					{
						// The collection is empty, so avoid generating invalid SQL syntax of "ColumnName IN ()".
						// Instead, just select from the null set via "ColumnName IN (NULL)"
						sIn.Append("NULL");
					}

					statement = string.Format("{0} {1} ({2})", quotedColName, "In", sIn);
					break;

				default:
					throw new NotSupportedException();
			}

			return new PartialSqlString(statement);
		}

		/// <summary>Visit SQL method call.</summary>
		/// <exception cref="NotSupportedException">Thrown when the requested operation is not supported.</exception>
		/// <param name="m">The MethodCallExpression to process.</param>
		/// <returns>An object.</returns>
		protected virtual object VisitSqlMethodCall(MethodCallExpression m)
		{
			var args = VisitSqlParameters(m.Arguments);
			var quotedColName = args.Dequeue();

			string statement;

			switch (m.Method.Name)
			{
				case "In":
					var member = Expression.Convert(m.Arguments[1], typeof(object));
					var lambda = Expression.Lambda<Func<object>>(member);
					var getter = lambda.Compile();

					var inArgs = Sql.Flatten(getter() as IEnumerable);

					var sIn = new StringBuilder();
					foreach (var e in inArgs)
					{
						var listArgs = e as ICollection;
						if (listArgs == null)
						{
							sIn.AppendFormat("{0}{1}",sIn.Length > 0 ? "," : string.Empty,AddParameter(e));
						}
						else
						{
							foreach (var el in listArgs)
							{
								sIn.AppendFormat("{0}{1}",sIn.Length > 0 ? "," : string.Empty, AddParameter(el));
							}
						}
					}

					statement = string.Format("{0} {1} ({2})", quotedColName, m.Method.Name, sIn);
					break;
				case "Desc":
					statement = string.Format("{0} DESC", quotedColName);
					break;
				case "As":
					statement = string.Format("{0} As {1}", quotedColName,
						DialectProvider.GetQuotedColumnName(RemoveQuoteFromAlias(args.Dequeue().ToString())));
					break;
				case "Sum":
				case "Count":
				case "Min":
				case "Max":
				case "Avg":
					statement = string.Format("{0}({1}{2})",
						m.Method.Name,
						quotedColName,
						args.Count == 1 ? string.Format(",{0}", args.Dequeue()) : string.Empty);
					break;
				default:
					throw new NotSupportedException();
			}

			return new PartialSqlString(statement);
		}
		protected virtual List<object> VisitInSqlExpressionList(ReadOnlyCollection<Expression> original)
		{
			var list = new List<object>();
			for (int i = 0, n = original.Count; i < n; i++)
			{
				var e = original[i];
				if (e.NodeType == ExpressionType.NewArrayInit ||
					 e.NodeType == ExpressionType.NewArrayBounds)
				{
					list.AddRange(VisitNewArrayFromExpressionList(e as NewArrayExpression));
				}
				else if (e.NodeType == ExpressionType.MemberAccess)
				{
					MemberExpression m  = e as MemberExpression;
					var propertyInfo = m.Member as PropertyInfo;
					if (propertyInfo != null && propertyInfo.PropertyType.IsEnum)
						list.Add(new EnumMemberAccess(
							 (PrefixFieldWithTableName ? DialectProvider.GetQuotedTableName(_modelDef.ModelName) + "." : string.Empty)
						+ GetQuotedColumnName(m.Member.Name), propertyInfo.PropertyType));
					else
					{
						list.Add(new PartialSqlString((PrefixFieldWithTableName ? DialectProvider.GetQuotedTableName(_modelDef.ModelName) + "." : string.Empty)
							+ GetQuotedColumnName(m.Member.Name)));
					}
				}
				else
				{
					list.Add(Visit(e));
				}
			}
			return list;
		}

		protected virtual Queue<object> VisitSqlParameters(ReadOnlyCollection<Expression> parameters)
		{
			var list = new Queue<object>();
			foreach(var e in parameters)
			{
				switch (e.NodeType)
				{
					case ExpressionType.NewArrayInit:
					case ExpressionType.NewArrayBounds:
						foreach (var p in VisitNewArrayFromExpressionList(e as NewArrayExpression))
						{
							list.Enqueue(p);
						}
						break;
					case ExpressionType.MemberAccess:
						MemberExpression m  = e as MemberExpression;
						var propertyInfo = m.Member as PropertyInfo;
						if (propertyInfo != null && propertyInfo.PropertyType.IsEnum)
							list.Enqueue(new EnumMemberAccess(
								(PrefixFieldWithTableName ? DialectProvider.GetQuotedTableName(_modelDef.ModelName) + "." : string.Empty)
								+ GetQuotedColumnName(m.Member.Name), propertyInfo.PropertyType));
						else
						{
							list.Enqueue(new PartialSqlString((PrefixFieldWithTableName ? DialectProvider.GetQuotedTableName(_modelDef.ModelName) + "." : string.Empty)
							                                  + GetQuotedColumnName(m.Member.Name)));
						}
						break;
					default:
						list.Enqueue(Visit(e));
						break;
				}
			}
			return list;
		}


		/// <summary>Visit column access method.</summary>
		/// <exception cref="NotSupportedException">Thrown when the requested operation is not supported.</exception>
		/// <param name="m">The MethodCallExpression to process.</param>
		/// <returns>An object.</returns>
		protected virtual object VisitColumnAccessMethod(MethodCallExpression m)
		{
			List<object> args = new List<object>();
			if (m.Arguments.Count == 1 && m.Arguments[0].NodeType == ExpressionType.Constant)
			{
				args.Add(((ConstantExpression)m.Arguments[0]).Value);
			}
			else
			{
				args.AddRange(VisitExpressionList(m.Arguments));
			}
			var quotedColName = Visit(m.Object);
			string statement;

			switch (m.Method.Name)
			{
				case "Trim":
					statement = string.Format("ltrim(rtrim({0}))", quotedColName);
					break;
				case "LTrim":
					statement = string.Format("ltrim({0})", quotedColName);
					break;
				case "RTrim":
					statement = string.Format("rtrim({0})", quotedColName);
					break;
				case "ToUpper":
					statement = string.Format("upper({0})", quotedColName);
					break;
				case "ToLower":
					statement = string.Format("lower({0})", quotedColName);
					break;
				case "StartsWith":
					statement = string.Format("upper({0}) LIKE {1} ", quotedColName,
						AddParameter(args[0].ToString().ToUpper() + "%"));
					break;
				case "EndsWith":
					statement = string.Format("upper({0}) LIKE {1}", quotedColName,
						AddParameter("%" + args[0].ToString().ToUpper()));
					break;
				case "Contains":
					statement = string.Format("upper({0}) LIKE {1}",
						quotedColName,
						AddParameter("%" + args[0].ToString().ToUpper() + "%"));
					break;
				case "Substring":
					var startIndex = int.Parse(args[0].ToString()) + 1;
					if (args.Count == 2)
					{
						var length = int.Parse(args[1].ToString());
						statement = string.Format("substring({0} from {1} for {2})",
							quotedColName,
							startIndex,
							length);
					}
					else
						statement = string.Format("substring({0} from {1})",
							quotedColName,
							startIndex);
					break;
				default:
					throw new NotSupportedException();
			}
			return new PartialSqlString(statement);
		}

		/// <summary>
		/// Add a parameter to the query and return corresponding parameter name
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		protected string AddParameter(object param)
		{
			string paramName = DialectProvider.GetParameterName(_parameters.Count);
			_parameters.Add(paramName, param);
			return paramName;
		}
	}

	/// <summary>A partial SQL string.</summary>
	public class PartialSqlString
	{
		/// <summary>
		///     Initializes a new instance of the NServiceKit.OrmLite.PartialSqlString class.
		/// </summary>
		/// <param name="text">The text.</param>
		public PartialSqlString(string text)
		{
			Text = text;
		}

		/// <summary>Gets or sets the text.</summary>
		/// <value>The text.</value>
		public string Text { get; set; }

		/// <summary>
		///     Returns a <see cref="T:System.String" /> that represents the current
		///     <see cref="T:System.Object" />.
		/// </summary>
		/// <returns>
		///     A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
		/// </returns>
		public override string ToString()
		{
			return Text;
		}
	}

	/// <summary>An enum member access.</summary>
	public class EnumMemberAccess : PartialSqlString
	{
		/// <summary>
		///     Initializes a new instance of the NServiceKit.OrmLite.EnumMemberAccess class.
		/// </summary>
		/// <exception cref="ArgumentException">
		///     Thrown when one or more arguments have unsupported or
		///     illegal values.
		/// </exception>
		/// <param name="text">    The text.</param>
		/// <param name="enumType">The type of the enum.</param>
		public EnumMemberAccess(string text, Type enumType)
			: base(text)
		{
			if (!enumType.IsEnum) throw new ArgumentException("Type not valid", "enumType");

			EnumType = enumType;
		}

		/// <summary>Gets the type of the enum.</summary>
		/// <value>The type of the enum.</value>
		public Type EnumType { get; private set; }
	}
}