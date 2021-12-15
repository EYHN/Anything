using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nito.Disposables;

namespace Anything.Database;

public class BaseTransaction : SingleDisposable<object?>, ITransaction
{
    private readonly Stack<Action> _rollbackStack = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="BaseTransaction" /> class.
    /// </summary>
    /// <param name="mode">Transaction mode.</param>
    public BaseTransaction(
        ITransaction.TransactionMode mode)
        : base(null)
    {
        Mode = mode;
    }

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
            catch (Exception)
            {
                Console.WriteLine("Rollback Error");
            }
        }
    }

    /// <summary>
    ///     Asynchronously applies the changes made in the transaction.
    /// </summary>
    public virtual Task CommitAsync()
    {
        EnsureNotCompleted();

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

        _rollbackStack.Clear();
        Completed = true;
    }

    /// <summary>
    ///     Asynchronously reverts the changes made in the transaction.
    /// </summary>
    public virtual Task RollbackAsync()
    {
        EnsureNotCompleted();

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

        DoRollbackWorks();
        _rollbackStack.Clear();
        Completed = true;
    }

    protected void EnsureNotCompleted()
    {
        if (Completed)
        {
            throw new InvalidOperationException("The transaction is completed.");
        }
    }

    protected override void Dispose(object? context)
    {
        if (!Completed)
        {
            Rollback();
        }
    }
}
