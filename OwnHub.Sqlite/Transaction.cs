using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using OwnHub.Utils;

namespace OwnHub.Sqlite
{
    public class Transaction: IDisposable, IAsyncDisposable
    {
        public enum TransactionMode
        {
            Query = 1,
            Mutation = 2,
        }

        private readonly Stack<Action> _rollbackStack = new();

        /// <summary>
        /// Gets the mode of this transaction
        /// </summary>
        public TransactionMode Mode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Transaction"/> class.
        /// </summary>
        /// <param name="mode">Transaction mode.</param>
        public Transaction(
            TransactionMode mode)
        {
            Mode = mode;
        }

        private void PushRollbackWork(Action func)
        {
            _rollbackStack.Push(func);
        }

        /// <summary>
        /// Running side effects can be rolled back when the transaction is rolled back.
        /// </summary>
        /// <param name="sideEffect">Side effects function.</param>
        /// <param name="rollback">Roll back function.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void RunSideEffect(Action sideEffect, Action rollback)
        {
            if (Mode != TransactionMode.Mutation)
            {
                throw new InvalidOperationException("Side effect only work in mutation mode.");
            }

            PushRollbackWork(rollback);
            sideEffect();
        }

        private void DoRollbackWorks()
        {
            foreach (var rollbackWork in _rollbackStack)
            {
                try
                {
                    rollbackWork();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Rollback Error: " + e);
                }
            }
        }

        /// <summary>
        /// Asynchronously applies the changes made in the transaction.
        /// </summary>
        public virtual Task CommitAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Applies the changes made in the transaction.
        /// </summary>
        public virtual void Commit()
        {
        }

        /// <summary>
        /// Asynchronously reverts the changes made in the transaction.
        /// </summary>
        public virtual Task RollbackAsync()
        {
            DoRollbackWorks();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Reverts the changes made in the transaction.
        /// </summary>
        public virtual void Rollback()
        {
            DoRollbackWorks();
        }

        public virtual void Dispose()
        {
        }

        public virtual ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
