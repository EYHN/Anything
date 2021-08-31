using System;
using System.Threading.Tasks;

namespace Anything.Database
{
    public interface ITransaction : IDisposable
    {
        public enum TransactionMode
        {
            Query,
            Mutation,
            Create
        }

        /// <summary>
        ///     Gets the mode of this transaction.
        /// </summary>
        public TransactionMode Mode { get; }

        public bool Completed { get; }

        public void PushRollbackWork(Action func);

        /// <summary>
        ///     Running side effects can be rolled back when the transaction is rolled back.
        /// </summary>
        /// <param name="sideEffect">Side effects function.</param>
        /// <param name="rollback">Roll back function.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void RunSideEffect(Action sideEffect, Action rollback);

        /// <summary>
        ///     Running side effects can be rolled back when the transaction is rolled back.
        /// </summary>
        /// <param name="sideEffect">Side effects function.</param>
        /// <param name="rollback">Roll back function.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public T RunSideEffect<T>(Func<T> sideEffect, Action rollback);

        public void DoRollbackWorks();

        /// <summary>
        ///     Asynchronously applies the changes made in the transaction.
        /// </summary>
        public Task CommitAsync();

        /// <summary>
        ///     Applies the changes made in the transaction.
        /// </summary>
        public void Commit();

        /// <summary>
        ///     Asynchronously reverts the changes made in the transaction.
        /// </summary>
        public Task RollbackAsync();

        /// <summary>
        ///     Reverts the changes made in the transaction.
        /// </summary>
        public void Rollback();
    }
}
