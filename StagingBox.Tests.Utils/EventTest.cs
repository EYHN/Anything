using System.Threading.Tasks;
using NUnit.Framework;
using StagingBox.Utils.Event;

namespace StagingBox.Tests.Utils
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
            @event.On(
                (e) =>
                {
                    Assert.AreEqual("hello", e);
                    callCount++;
                });
            @event.On(
                async (e) =>
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
