using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace SimpleStack.Orm
{
	public class OrmConnection : IDbConnection
	{
		public IDialectProvider DialectProvider { get; }

		public int CommandTimeout { get; set; }

		/// <summary>Gets the transaction.</summary>
		/// <value>The transaction.</value>
		public OrmTransaction Transaction { get; internal set; }

		/// <summary>true if this object is open.</summary>
		private bool _isOpen;

		/// <summary>
		/// Initializes a new instance of the OrmLiteConnection class.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="dialectProvider"></param>
		public OrmConnection(IDbConnection connection, IDialectProvider dialectProvider)
		{
			DialectProvider = dialectProvider;
			DbConnection = connection;
		}

		/// <summary>Gets the database connection.</summary>
		/// <value>The database connection.</value>
		public IDbConnection DbConnection { get; private set; }

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
		/// resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (DbConnection != null)
				{
					DbConnection.Dispose();
					DbConnection = null;
				}
				_isOpen = false;
			}
		}

		/// <summary>Begins a database transaction.</summary>
		/// <returns>An object representing the new transaction.</returns>
		public IDbTransaction BeginTransaction()
		{
			Transaction = new OrmTransaction(this, DbConnection.BeginTransaction());
			return Transaction;
		}

		/// <summary>
		/// Begins a database transaction with the specified <see cref="T:System.Data.IsolationLevel" />
		/// value.
		/// </summary>
		/// <param name="isolationLevel">The isolation level.</param>
		/// <returns>An object representing the new transaction.</returns>
		/// ### <param name="il">One of the <see cref="T:System.Data.IsolationLevel" /> values.</param>
		public IDbTransaction BeginTransaction(IsolationLevel isolationLevel)
		{
			Transaction = new OrmTransaction(this, DbConnection.BeginTransaction(isolationLevel));
			return Transaction;
		}

		/// <summary>Closes the connection to the database.</summary>
		public void Close()
		{
			DbConnection.Close();
			_isOpen = false;
		}

		/// <summary>Changes the current database for an open Connection object.</summary>
		/// <param name="databaseName">The name of the database to use in place of the current database.</param>
		public void ChangeDatabase(string databaseName)
		{
			DbConnection.ChangeDatabase(databaseName);
		}

		/// <summary>Creates and returns a Command object associated with the connection.</summary>
		/// <returns>A Command object associated with the connection.</returns>
		public IDbCommand CreateCommand()
		{
			var cmd = new OrmCommand(DbConnection.CreateCommand());
			if (Transaction != null)
			{
				cmd.Transaction = Transaction.trans;
			}
			cmd.CommandTimeout = CommandTimeout;
			return cmd;
		}

		/// <summary>
		/// Opens a database connection with the settings specified by the ConnectionString property of
		/// the provider-specific Connection object.
		/// </summary>
		public void Open()
		{
			if (_isOpen)
				return;

			DbConnection.Open();
			_isOpen = true;
		}

		/// <summary>Gets or sets the string used to open a database.</summary>
		/// <value>A string containing connection settings.</value>
		public string ConnectionString
		{
			get { return DbConnection.ConnectionString; }
			set { DbConnection.ConnectionString = value; }
		}

		/// <summary>
		/// Gets the time to wait while trying to establish a connection before terminating the attempt
		/// and generating an error.
		/// </summary>
		/// <value>
		/// The time (in seconds) to wait for a connection to open. The default value is 15 seconds.
		/// </value>
		public int ConnectionTimeout => DbConnection.ConnectionTimeout;

		/// <summary>
		/// Gets the name of the current database or the database to be used after a connection is opened.
		/// </summary>
		/// <value>
		/// The name of the current database or the name of the database to be used once a connection is
		/// open. The default value is an empty string.
		/// </value>
		public string Database => DbConnection.Database;

		/// <summary>Gets the current state of the connection.</summary>
		/// <value>One of the <see cref="T:System.Data.ConnectionState" /> values.</value>
		public ConnectionState State => DbConnection.State;
	}
}