using System;
using System.IO;
using Anything.Utils;
using NUnit.Framework;

namespace Anything.Tests.Utils
{
    public class DisposableTest
    {
        [Test]
        public void CallbackTest()
        {
            var flag = false;
            var dispose = new Disposable(() => flag = true);
            Assert.AreEqual(false, flag);
            dispose.Dispose();
            Assert.AreEqual(true, flag);
        }

        [Test]
        public void FromTest()
        {
            var flag = false;
            var childDisposable = new Disposable(() => flag = true);
            var dispose = Disposable.From(childDisposable);
            dispose.Dispose();
            Assert.AreEqual(true, flag);
        }
    }
}
