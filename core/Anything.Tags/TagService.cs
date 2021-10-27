using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Fork;
using Anything.Utils;
using Microsoft.EntityFrameworkCore;

namespace Anything.Tags
{
    public class TagService : Disposable, ITagService
    {
        private readonly EfCoreFileForkService _fileForkService;

        public TagService(IFileService fileService, IStorage storage)
        {
            _fileForkService = new EfCoreFileForkService(
                fileService,
                "tag",
                storage.EfCoreFileForkStorage,
                new[] { typeof(TagEntity) });
        }

        public async ValueTask<Tag[]> GetTags(FileHandle fileHandle)
        {
            await using var context = _fileForkService.CreateContext();
            return await context.Set<TagEntity>().AsQueryable()
                .Where(t => t.File.FileHandle == fileHandle)
                .Select(t => new Tag(t.Name))
                .ToArrayAsync();
        }

        public async ValueTask AddTags(FileHandle fileHandle, Tag[] tags)
        {
            await using var context = _fileForkService.CreateContext();
            var file = await context.GetOrCreateFileEntity(fileHandle);
            await context.Set<TagEntity>().AddRangeAsync(tags.Select(t => new TagEntity { Name = t.Name, File = file }));
            await context.SaveChangesAsync();
        }

        public async ValueTask RemoveTags(FileHandle fileHandle, Tag[] tags)
        {
            await using var context = _fileForkService.CreateContext();
            var file = await context.GetOrCreateFileEntity(fileHandle);
            var tagEntity = context.Set<TagEntity>();
            var tagNames = tags.Select(tag => tag.Name);
            tagEntity.RemoveRange(tagEntity.AsQueryable().Where(t => tagNames.Any(tag => tag == t.Name) && t.File == file));
            await context.SaveChangesAsync();
        }

        protected override void DisposeManaged()
        {
            base.DisposeManaged();

            _fileForkService.Dispose();
        }

        [Table("tag")]
        public class TagEntity : EfCoreFileForkService.FileForkEntity
        {
            public int Id { get; set; }

            public string Name { get; set; } = null!;
        }

        public interface IStorage
        {
            public EfCoreFileForkService.IStorage EfCoreFileForkStorage { get; }
        }

        public class MemoryStorage : Disposable, IStorage
        {
            private readonly EfCoreFileForkService.MemoryStorage _memoryStorage;

            public EfCoreFileForkService.IStorage EfCoreFileForkStorage => _memoryStorage;

            public MemoryStorage()
            {
                _memoryStorage = new EfCoreFileForkService.MemoryStorage();
            }

            protected override void DisposeManaged()
            {
                base.DisposeManaged();

                _memoryStorage.Dispose();
            }
        }
    }
}
