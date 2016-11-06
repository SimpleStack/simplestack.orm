using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
//using SimpleStack.Orm.Logging;

namespace SimpleStack.Orm
{
	public partial class OrmConnection : DbConnection
	{
		//private static readonly ILog Logger = LogProvider.For<OrmConnection>();

		public IDialectProvider DialectProvider { get; }

		public int CommandTimeout { get; set; }

		///// <summary>Gets the transaction.</summary>
		///// <value>The transaction.</value>
		public OrmTransaction Transaction { get; internal set; }

		///// <summary>true if this object is open.</summary>
		private bool _isOpen;

		///// <summary>
		///// Initializes a new instance of the OrmLiteConnection class.
		///// </summary>
		///// <param name="connection"></param>
		///// <param name="dialectProvider"></param>
		internal OrmConnection(DbConnection connection, IDialectProvider dialectProvider)
		{
			DialectProvider = dialectProvider;
			DbConnection = connection;
		}

		///// <summary>Gets the database connection.</summary>
		///// <value>The database connection.</value>
		public DbConnection DbConnection { get; private set; }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (DbConnection != null)
				{
					Close();

					DbConnection.Dispose();
					DbConnection = null;
				}
				_isOpen = false;
			}
			base.Dispose(disposing);
		}

		internal void ClearCurrrentTransaction()
		{
			Transaction = null;
		}

		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
		{
			Transaction = new OrmTransaction(this, DbConnection.BeginTransaction());
			return Transaction;
		}

		public override void Close()
		{
			if (_isOpen)
			{
				//Logger.DebugFormat("Closing Connection");
				DbConnection.Close();
				_isOpen = false;
			}
		}

		public override void ChangeDatabase(string databaseName)
		{
			DbConnection.ChangeDatabase(databaseName);
		}

		public override void Open()
		{
			//Logger.DebugFormat("Opening Connection");
			DbConnection.Open();
			_isOpen = true;
		}

		public override string ConnectionString
		{
			get { return DbConnection.ConnectionString; }
			set { DbConnection.ConnectionString = value; }
		}

		public override string Database => DbConnection.Database;

		public override ConnectionState State => DbConnection.State;
		public override string DataSource => DbConnection.DataSource;
		public override string ServerVersion => DbConnection.ServerVersion;

		protected override DbCommand CreateDbCommand()
		{
			var cmd = new OrmCommand(DbConnection.CreateCommand());
			if (Transaction != null)
			{
				cmd.Transaction = Transaction.trans;
			}
			cmd.CommandTimeout = CommandTimeout;
			return cmd;
		}
	}
}