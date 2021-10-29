using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Fork;
using Anything.Utils;
using Microsoft.EntityFrameworkCore;

namespace Anything.Notes
{
    public class NoteService : Disposable, INoteService
    {
        private readonly EfCoreFileForkService _fileForkService;

        public NoteService(IFileService fileService, IStorage storage)
        {
            _fileForkService = new EfCoreFileForkService(
                fileService,
                "note",
                storage.EfCoreFileForkStorage,
                new[] { typeof(NoteEntity) });
        }

        public async ValueTask<string> GetNotes(FileHandle fileHandle)
        {
            await using var context = _fileForkService.CreateContext();
            var entity = await context.Set<NoteEntity>().AsQueryable()
                .SingleOrDefaultAsync(n => n.File.FileHandle == fileHandle);
            return entity == null ? "" : entity.Content;
        }

        public async ValueTask SetNotes(FileHandle fileHandle, string notes)
        {
            await using var context = _fileForkService.CreateContext();
            var file = await context.GetOrCreateFileEntity(fileHandle);
            var noteEntity = context.Set<NoteEntity>();
            noteEntity.RemoveRange(noteEntity.AsQueryable().Where((n) => n.File == file));
            await noteEntity.AddAsync(new NoteEntity { Content = notes, File = file });
            await context.SaveChangesAsync();
        }

        protected override void DisposeManaged()
        {
            base.DisposeManaged();

            _fileForkService.Dispose();
        }

        [Table("note")]
        public class NoteEntity : EfCoreFileForkService.FileForkEntity
        {
            public int Id { get; set; }

            public string Content { get; set; } = null!;
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
