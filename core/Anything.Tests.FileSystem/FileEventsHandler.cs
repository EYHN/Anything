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
            Assert.IsTrue(
                expectedEvents.Length == _eventsCache.Count && expectedEvents.All(
                    expected =>
                    {
                        var expectedMetadata =
                            expected.AttachedData.Select(r => r.Payload + ':' + r.DeletionPolicy).OrderBy(t => t).ToArray();
                        return _eventsCache.Any(
                            e =>
                            {
                                var trackers =
                                    e.AttachedData.Select(r => r.Payload + ':' + r.DeletionPolicy).OrderBy(t => t).ToArray();

                                return e.Url == expected.Url && e.Type == expected.Type &&
                                       string.Join(',', trackers) == string.Join(
                                           ',',
                                           expectedMetadata);
                            });
                    }));
            _eventsCache.Clear();
        }

        public void HandleFileEvents(FileEvent[] fileEvents)
        {
            _eventsCache.AddRange(fileEvents);
        }
    }
}
