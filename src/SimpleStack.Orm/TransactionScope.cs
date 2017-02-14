using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#if !NETSTANDARD1_5
using System.Runtime.Remoting.Messaging;
#endif

namespace SimpleStack.Orm
{

	public class TransactionScope : IDisposable
	{
#if NETSTANDARD1_5
		private readonly AsyncLocal<TransactionScope> _rootConnectionScope = new AsyncLocal<TransactionScope>();
#endif
		private OrmConnection _rootScopeConnection;
		private IDbTransaction _transaction;

		public TransactionScope(OrmConnectionFactory connectionFactory)
		{
			if (RootScope == null)
			{
				// Create DB Connection scope
				_rootScopeConnection = connectionFactory.OpenConnection();

				// Create a transaction
				_transaction = _rootScopeConnection.BeginTransaction();

				// Setup self as the root scope
				RootScope = this;
			}

			Completed = false;
		}

		public TransactionScope RootScope
		{
#if NETSTANDARD1_5
			get { return _rootConnectionScope.Value; }
			set { _rootConnectionScope.Value = value; }
#else
			get { return (TransactionScope)CallContext.LogicalGetData("_rootScope"); }
			set { CallContext.LogicalSetData("_rootScope",value); }
#endif
		}

		public bool Completed { get; private set; }

		public OrmConnection Connection => RootScope.Connection;

		public void Complete()
		{
			// Set completed flag
			Completed = true;

			// Don't complete if we're not the root scope.
			if (RootScope != this)
			{
				return;
			}

			//Logger.DebugFormat("Committing Transaction");
			_transaction?.Commit();

			Cleanup();
		}

		public void Cancel()
		{
			// If we are not the root scope, cancel the entire transaction
			if (RootScope != null && RootScope != this)
			{
				RootScope.Cancel();
				return;
			}

			_transaction?.Rollback();

			Cleanup();
		}

		public void Dispose()
		{
			if (!Completed)
			{
				Cancel();
			}
		}

		private void Cleanup()
		{
			if (_transaction != null)
			{
				_transaction.Dispose();
				_transaction = null;
			}

			if (_rootScopeConnection != null)
			{
				_rootScopeConnection.Dispose();
				_rootScopeConnection = null;
			}

			RootScope = null;
		}
	}

}
