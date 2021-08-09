using System.Collections.Generic;
using System.Linq;
using Anything.FileSystem.Tracker;
using NUnit.Framework;

namespace Anything.Tests.FileSystem
{
    public class FileEventsHandler
    {
        private readonly List<FileEvent> _eventsCache = new();

        public void AssertWithEvent(FileEvent[] expectedEvents)
        {
            var expected = string.Join('\n', expectedEvents.Select(item => "\t" + item));
            var cached = string.Join('\n', _eventsCache.Select(item => "\t" + item));
            var message = $"\nExpected:\n{expected}\nBut was:\n{cached}";

            Assert.AreEqual(expectedEvents.Length, _eventsCache.Count, message);
            Assert.IsTrue(!expectedEvents.Except(_eventsCache).Any(), message);
            _eventsCache.Clear();
        }

        public void HandleFileEvents(FileEvent[] fileEvents)
        {
            _eventsCache.AddRange(fileEvents);
        }
    }
}
