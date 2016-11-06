using System.Data;
using System.Data.Common;
//using SimpleStack.Orm.Logging;

namespace SimpleStack.Orm
{
	public class OrmTransaction : DbTransaction
	{
		//private static readonly ILog Logger = LogProvider.For<OrmTransaction>();

		/// <summary>The transaction.</summary>
		internal readonly DbTransaction trans;

		/// <summary>The database.</summary>
		private readonly OrmConnection db;

		private bool _isOpen;

		/// <summary>
		/// Initializes a new instance of the NServiceKit.OrmLite.OrmLiteTransaction class.
		/// </summary>
		/// <param name="db">   The database.</param>
		/// <param name="trans">The transaction.</param>
		internal OrmTransaction(OrmConnection db, DbTransaction trans)
		{
			//Logger.Debug("Begin Transaction");
			this.db = db;
			this.trans = trans;
			_isOpen = true;
		}
		
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_isOpen)
				{
					Rollback();
				}

				trans.Dispose();
				db.ClearCurrrentTransaction();
				base.Dispose(true);
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
		public override void Commit()
		{
			//Logger.Debug("Commit");
			trans.Commit();
			db.ClearCurrrentTransaction();
			_isOpen = false;
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
		public override void Rollback()
		{
			//Logger.Debug("Rollback");
			trans.Rollback();
			db.ClearCurrrentTransaction();
			_isOpen = false;
		}

		protected override DbConnection DbConnection => trans.Connection;
		public override IsolationLevel IsolationLevel => trans.IsolationLevel;
	}
}