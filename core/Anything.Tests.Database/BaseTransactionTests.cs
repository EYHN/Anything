using System;
using System.Threading.Tasks;
using Anything.Database;
using NUnit.Framework;

namespace Anything.Tests.Database;

public class BaseTransactionTests
{
    [Test]
    public void QueryTest()
    {
        var a = 0;
        using var transaction = new BaseTransaction(ITransaction.TransactionMode.Query);

        Assert.Catch<InvalidOperationException>(() => transaction.RunSideEffect(() => ++a, () => a--));
        Assert.Catch<InvalidOperationException>(() => transaction.RunSideEffect(Assert.Fail, Assert.Fail));
    }

    [Test]
    public void SideEffectTest()
    {
        var a = 0;
        using var transaction = new BaseTransaction(ITransaction.TransactionMode.Create);

        Assert.AreEqual(1, transaction.RunSideEffect(() => ++a, () => a--));

        Assert.AreEqual(1, a);

        transaction.RunSideEffect(() => { ++a; }, () => { a--; });

        Assert.AreEqual(2, a);

        transaction.Commit();

        Assert.AreEqual(2, a);
    }

    [Test]
    public void SideEffectRollbackTest()
    {
        var a = 0;
        using var transaction = new BaseTransaction(ITransaction.TransactionMode.Create);

        transaction.RunSideEffect(
            () =>
            {
                Assert.AreEqual(0, a);
                a++;
            },
            () =>
            {
                Assert.AreEqual(1, a);
                a--;
            });

        transaction.RunSideEffect(
            () =>
            {
                Assert.AreEqual(1, a);
                a++;
            },
            () =>
            {
                Assert.AreEqual(2, a);
                a--;
            });

        transaction.Rollback();

        Assert.AreEqual(0, a);
    }

    [Test]
    public async Task AsyncTest()
    {
        var a = 0;
        using var transaction1 = new BaseTransaction(ITransaction.TransactionMode.Create);
        transaction1.RunSideEffect(() => a++, () => a--);
        Assert.AreEqual(1, a);
        await transaction1.RollbackAsync();
        Assert.AreEqual(0, a);

        using var transaction2 = new BaseTransaction(ITransaction.TransactionMode.Create);
        transaction2.RunSideEffect(() => a++, () => a--);
        Assert.AreEqual(1, a);
        await transaction2.CommitAsync();
        Assert.AreEqual(1, a);
    }

    [Test]
    public void DisposeTest()
    {
        var a = 0;
        using (var transaction = new BaseTransaction(ITransaction.TransactionMode.Create))
        {
            transaction.RunSideEffect(() => a++, () => a--);
            Assert.AreEqual(1, a);
        }

        Assert.AreEqual(0, a);

        using (var transaction = new BaseTransaction(ITransaction.TransactionMode.Create))
        {
            transaction.RunSideEffect(() => a++, () => a--);
            Assert.AreEqual(1, a);

            transaction.Commit();
        }

        Assert.AreEqual(1, a);
    }

    [Test]
    public void CompletedTest()
    {
        using var transaction = new BaseTransaction(ITransaction.TransactionMode.Create);

        Assert.AreEqual(false, transaction.Completed);

        transaction.Commit();

        Assert.AreEqual(true, transaction.Completed);

        Assert.Catch<InvalidOperationException>(() => transaction.Commit());
    }
}
