using System.Threading.Tasks;
using Anything.Utils.Event;
using NUnit.Framework;

namespace Anything.Tests.Utils
{
    public class EventTest
    {
        [Test]
        [Timeout(3000)]
        public async Task FeatureTest()
        {
            EventEmitter<string> emitter = new();
            var @event = emitter.Event;
            var callCount = 0;
            using var event1 = @event.On(
                e =>
                {
                    Assert.AreEqual("hello", e);
                    callCount++;
                });
            using var event2 = @event.On(
                async e =>
                {
                    Assert.AreEqual("hello", e);
                    await Task.Delay(100);
                    callCount++;
                });
            Assert.AreEqual(0, callCount);
            emitter.Emit("hello");
            Assert.AreEqual(2, callCount);

            await emitter.EmitAsync("hello");
            Assert.AreEqual(4, callCount);
        }
    }
}
