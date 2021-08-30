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
            using (new Disposable(() => flag = true))
            {
                Assert.AreEqual(false, flag);
            }

            Assert.AreEqual(true, flag);
        }

        [Test]
        public void FromTest()
        {
            var flag = false;
            using var childDisposable = new Disposable(() => flag = true);
            using (Disposable.From(childDisposable))
            {
            }

            Assert.AreEqual(true, flag);
        }
    }
}
