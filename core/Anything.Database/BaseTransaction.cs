using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Anything.Database
{
    public class BaseTransaction : ITransaction
    {
        private readonly Stack<Action> _rollbackStack = new();

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseTransaction" /> class.
        /// </summary>
        /// <param name="mode">Transaction mode.</param>
        /// <param name="logger">Logger for the transaction.</param>
        public BaseTransaction(
            ITransaction.TransactionMode mode,
            ILogger? logger = null)
        {
            Mode = mode;
            Logger = logger;
        }

        protected ILogger? Logger { get; }

        public bool Completed { get; private set; }

        /// <summary>
        ///     Gets the mode of this transaction.
        /// </summary>
        public ITransaction.TransactionMode Mode { get; }

        public void PushRollbackWork(Action func)
        {
            EnsureNotCompleted();

            _rollbackStack.Push(func);
        }

        /// <summary>
        ///     Running side effects can be rolled back when the transaction is rolled back.
        /// </summary>
        /// <param name="sideEffect">Side effects function.</param>
        /// <param name="rollback">Roll back function.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void RunSideEffect(Action sideEffect, Action rollback)
        {
            EnsureNotCompleted();

            if (Mode == ITransaction.TransactionMode.Query)
            {
                throw new InvalidOperationException("Side effect cannot work in query mode.");
            }

            PushRollbackWork(rollback);
            sideEffect();
        }

        /// <summary>
        ///     Running side effects can be rolled back when the transaction is rolled back.
        /// </summary>
        /// <param name="sideEffect">Side effects function.</param>
        /// <param name="rollback">Roll back function.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public T RunSideEffect<T>(Func<T> sideEffect, Action rollback)
        {
            EnsureNotCompleted();

            if (Mode == ITransaction.TransactionMode.Query)
            {
                throw new InvalidOperationException("Side effect cannot work in query mode.");
            }

            PushRollbackWork(rollback);
            return sideEffect();
        }

        public void DoRollbackWorks()
        {
            EnsureNotCompleted();

            foreach (var rollbackWork in _rollbackStack)
            {
                try
                {
                    rollbackWork();
                }
                catch (Exception e)
                {
                    Logger?.LogError(e, "Rollback Error");
                }
            }
        }

        /// <summary>
        ///     Asynchronously applies the changes made in the transaction.
        /// </summary>
        public virtual Task CommitAsync()
        {
            EnsureNotCompleted();

            Logger?.LogTrace("Commit");
            _rollbackStack.Clear();
            Completed = true;
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Applies the changes made in the transaction.
        /// </summary>
        public virtual void Commit()
        {
            EnsureNotCompleted();

            Logger?.LogTrace("Commit");
            _rollbackStack.Clear();
            Completed = true;
        }

        /// <summary>
        ///     Asynchronously reverts the changes made in the transaction.
        /// </summary>
        public virtual Task RollbackAsync()
        {
            EnsureNotCompleted();

            Logger?.LogTrace("Rollback");
            DoRollbackWorks();
            _rollbackStack.Clear();
            Completed = true;
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Reverts the changes made in the transaction.
        /// </summary>
        public virtual void Rollback()
        {
            EnsureNotCompleted();

            Logger?.LogTrace("Rollback");
            DoRollbackWorks();
            _rollbackStack.Clear();
            Completed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public ValueTask DisposeAsync()
        {
            Dispose(true);
            return ValueTask.CompletedTask;
        }

        protected void EnsureNotCompleted()
        {
            if (Completed)
            {
                throw new InvalidOperationException("The transaction is completed.");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (!Completed)
                    {
                        Rollback();
                    }
                }

                _disposed = true;
            }
        }

        ~BaseTransaction()
        {
            Dispose(false);
        }
    }
}
