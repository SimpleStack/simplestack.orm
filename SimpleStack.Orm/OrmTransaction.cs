using System.Data;

namespace SimpleStack.Orm
{
	public class OrmTransaction : IDbTransaction
	{
		/// <summary>The previous transaction.</summary>
		private readonly OrmTransaction prevTrans;

		/// <summary>The transaction.</summary>
		internal readonly IDbTransaction trans;

		/// <summary>The database.</summary>
		private readonly OrmConnection db;

		/// <summary>
		/// Initializes a new instance of the NServiceKit.OrmLite.OrmLiteTransaction class.
		/// </summary>
		/// <param name="db">   The database.</param>
		/// <param name="trans">The transaction.</param>
		internal OrmTransaction(OrmConnection db, IDbTransaction trans)
		{
			this.db = db;
			this.trans = trans;
		}

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
				trans.Dispose();
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