using System.Data;

namespace SimpleStack.Orm
{
	public class OrmLiteTransaction : IDbTransaction
	{
		/// <summary>The previous transaction.</summary>
		private readonly OrmLiteTransaction prevTrans;

		/// <summary>The transaction.</summary>
		internal readonly IDbTransaction trans;

		/// <summary>The database.</summary>
		private readonly IDbConnection db;

		/// <summary>
		/// Initializes a new instance of the NServiceKit.OrmLite.OrmLiteTransaction class.
		/// </summary>
		/// <param name="db">   The database.</param>
		/// <param name="trans">The transaction.</param>
		public OrmLiteTransaction(IDbConnection db, IDbTransaction trans)
		{
			this.db = db;
			this.trans = trans;
			prevTrans = Config.TSTransaction;
			Config.TSTransaction = this;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
		/// resources.
		/// </summary>
		public void Dispose()
		{
			try
			{
				trans.Dispose();
			}
			finally
			{
				Config.TSTransaction = prevTrans;
				var ormLiteDbConn = this.db as OrmLiteConnection;
				if (ormLiteDbConn != null)
				{
					ormLiteDbConn.Transaction = prevTrans;
				}
			}
		}

		/// <summary>Commits the database transaction.</summary>
		/// ### <exception cref="T:System.Exception">                An error occurred while trying to
		/// commit the transaction.</exception>
		/// ### <exception cref="T:System.InvalidOperationException">The transaction has already been
		/// committed or rolled back.
		/// 
		///                     -or-
		/// 
		///                     The connection is broken.</exception>
		public void Commit()
		{
			trans.Commit();
		}

		/// <summary>Rolls back a transaction from a pending state.</summary>
		/// ### <exception cref="T:System.Exception">                An error occurred while trying to
		/// commit the transaction.</exception>
		/// ### <exception cref="T:System.InvalidOperationException">The transaction has already been
		/// committed or rolled back.
		/// 
		///                     -or-
		/// 
		///                     The connection is broken.</exception>
		public void Rollback()
		{
			trans.Rollback();
		}

		/// <summary>Specifies the Connection object to associate with the transaction.</summary>
		/// <value>The Connection object to associate with the transaction.</value>
		public IDbConnection Connection
		{
			get { return trans.Connection; }
		}

		/// <summary>
		/// Specifies the <see cref="T:System.Data.IsolationLevel" /> for this transaction.
		/// </summary>
		/// <value>
		/// The <see cref="T:System.Data.IsolationLevel" /> for this transaction. The default is
		/// ReadCommitted.
		/// </value>
		public IsolationLevel IsolationLevel
		{
			get { return trans.IsolationLevel; }
		}
	}
}