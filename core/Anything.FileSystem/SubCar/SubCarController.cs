using System.Collections.Generic;
using System.Threading.Tasks;
using Anything.FileSystem.Tracker;
using Anything.Utils;

namespace Anything.FileSystem.SubCar
{
    public abstract class SubCarController<TEntry, TParameter>
    {
        private readonly FileAttachedData.DeletionPolicies _deletionPolicy;

        private readonly IFileService _fileService;

        protected SubCarController(IFileService fileService, FileAttachedData.DeletionPolicies deletionPolicy)
        {
            _fileService = fileService;
            _fileService.FileEvent.On(HandleFileEvent);
            _deletionPolicy = deletionPolicy;
        }

        public abstract string Name { get; }

        private string SubCarPrefix => $"S:{Name}:";

        protected abstract Task<TEntry[]> Create(TParameter[] parameters);

        protected abstract Task Delete(TEntry[] entries);

        protected abstract string Serialize(TEntry entry);

        protected abstract TEntry Deserialize(string payload);

        private async Task HandleFileEvent(FileEvent[] fileEvents)
        {
            var entries = new List<TEntry>();
            foreach (var fileEvent in fileEvents)
            {
                if (fileEvent.Type is FileEvent.EventType.Changed or FileEvent.EventType.Deleted)
                {
                    foreach (var attachedData in fileEvent.AttachedData)
                    {
                        if (attachedData.Payload.StartsWith(SubCarPrefix))
                        {
                            var data = attachedData.Payload.Substring(SubCarPrefix.Length);
                            var entry = Deserialize(data);
                            entries.Add(entry);
                        }
                    }
                }
            }

            await Delete(entries.ToArray());
        }

        public async Task Attach(Url url, TParameter entry)
        {
            var fileRecord = FileRecord.FromFileStats(await _fileService.Stat(url));
            await Attach(url, fileRecord, entry);
        }

        public async Task Attach(Url url, FileRecord fileRecord, TParameter parameter)
        {
            var entries = await Create(new[] { parameter });
            foreach (var entry in entries)
            {
                var serialized = Serialize(entry);
                var data = SubCarPrefix + serialized;
                await _fileService.AttachData(url, fileRecord, new FileAttachedData(data, _deletionPolicy));
            }
        }
    }
}
