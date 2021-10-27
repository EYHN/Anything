using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Tracker;
using Anything.Utils;
using Anything.Utils.Event;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Anything.Fork
{
    public class EfCoreFileForkService : Disposable
    {
        private readonly IFileService _fileService;
        private readonly string _forkName;
        private readonly EventDisposable _attachDataEventDisposable;
        private readonly DbContextOptions _dbContextOptions;
        private readonly Action<ModelBuilder>? _onBuildModel;

        private string AttachDataPayload => "Fork:" + _forkName;

        public EfCoreFileForkService(
            IFileService fileService,
            string forkName,
            IStorage storage,
            Type[] entityTypes,
            Action<ModelBuilder>? onBuildModel = null)
        {
            _onBuildModel = onBuildModel;
            _fileService = fileService;
            _forkName = forkName;
            _attachDataEventDisposable = fileService.AttachDataEvent.On(OnAttachDataEvent);
            _dbContextOptions = BuildDbContextOptions(storage, entityTypes);

            using var dbContext = CreateContext();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            dbContext.SaveChanges();
        }

        public Context CreateContext()
        {
            return new(this);
        }

        [Table("file")]
        public class FileEntity
        {
            public int Id { get; set; }

            public FileHandle FileHandle { get; set; } = null!;
        }

        public class FileForkEntity
        {
            public int FileId { get; set; }

            [Required]
            public FileEntity File { get; set; } = null!;
        }

        public class Context : DbContext
        {
            public EfCoreFileForkService Service { get; }

            internal Context(EfCoreFileForkService service)
                : base(service._dbContextOptions)
            {
                Service = service;
            }

            public async ValueTask<FileEntity> GetOrCreateFileEntity(FileHandle fileHandle)
            {
                var files = Set<FileEntity>();
                var file = await files.AsQueryable().Where(f => f.FileHandle == fileHandle).FirstOrDefaultAsync();
                if (file != null)
                {
                    return file;
                }

                var fileEntity = await files.AddAsync(new FileEntity { FileHandle = fileHandle });

                await Service._fileService.AttachData(fileHandle, new FileAttachedData(Service.AttachDataPayload));
                return fileEntity.Entity;
            }
        }

        private async Task OnAttachDataEvent(AttachDataEvent[] events)
        {
            var removedFileHandles = new List<FileHandle>();
            foreach (var @event in events)
            {
                if (@event.Type == AttachDataEvent.EventType.Deleted && @event.AttachedData.Payload == AttachDataPayload)
                {
                    removedFileHandles.Add(@event.FileHandle);
                }
            }

            await using var dbContext = CreateContext();
            var files = dbContext.Set<FileEntity>();
            files.RemoveRange(files.AsQueryable().Where(f => removedFileHandles.Contains(f.FileHandle)));
            await dbContext.SaveChangesAsync();
        }

        private DbContextOptions BuildDbContextOptions(IStorage storage, Type[] entityTypes)
        {
            var conventions = SqliteConventionSetBuilder.Build();
            var modelBuilder = new ModelBuilder(conventions);

            var fileHandleConverter = new ValueConverter<FileHandle, string>(h => h.Identifier, h => new FileHandle(h, null));

            var fileHashConverter = new ValueConverter<FileHash, string>(h => h.ContentTag, h => new FileHash(h));

            var fileEntityBuilder = modelBuilder.Entity<FileEntity>();
            fileEntityBuilder.HasAlternateKey(f => f.FileHandle);
            fileEntityBuilder.Property(f => f.FileHandle).HasConversion(fileHandleConverter);

            foreach (var entityType in entityTypes)
            {
                if (!entityType.IsAssignableTo(typeof(FileForkEntity)))
                {
                    throw new NotSupportedException();
                }

                var entityBuilder = modelBuilder.Entity(entityType);

                foreach (var property in entityType.GetProperties())
                {
                    if (property.PropertyType == typeof(FileHash))
                    {
                        entityBuilder.Property(property.Name).HasConversion(fileHashConverter);
                    }
                }

                entityBuilder.HasOne(typeof(FileEntity), nameof(FileForkEntity.File)).WithMany().OnDelete(DeleteBehavior.Cascade);
            }

            if (_onBuildModel != null)
            {
                _onBuildModel(modelBuilder);
            }

            var model = modelBuilder.Model.FinalizeModel();
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder
                .UseModel(model)
                .LogTo(Console.WriteLine);
            storage.OnConfiguring(optionsBuilder);
            return optionsBuilder.Options;
        }

        protected override void DisposeManaged()
        {
            base.DisposeManaged();

            _attachDataEventDisposable.Dispose();
        }

        public interface IStorage
        {
            public void OnConfiguring(DbContextOptionsBuilder optionsBuilder);
        }

        public sealed class MemoryStorage : Disposable, IStorage
        {
            private static int _memoryConnectionSequenceId;
            private readonly SqliteConnection _connection;
            private readonly string _connectionString;

            public MemoryStorage()
            {
                var name =
                    $"ef-core-file-fork-memory-storage-{Interlocked.Increment(ref _memoryConnectionSequenceId)}-{DateTimeOffset.Now.ToUnixTimeMilliseconds()}";

                _connectionString = new SqliteConnectionStringBuilder
                {
                    Mode = SqliteOpenMode.Memory, DataSource = name, Cache = SqliteCacheMode.Shared
                }.ToString();

                _connection = new SqliteConnection(_connectionString);
                _connection.Open();
            }

            void IStorage.OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlite(_connectionString);
            }

            protected override void DisposeManaged()
            {
                base.DisposeManaged();

                _connection.Dispose();
            }
        }

        public sealed class LocalStorage : IStorage
        {
            private readonly string _dbFile;

            public LocalStorage(string dbFile)
            {
                _dbFile = dbFile;
            }

            void IStorage.OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlite(new SqliteConnectionStringBuilder
                {
                    Mode = SqliteOpenMode.ReadWriteCreate, DataSource = _dbFile, Cache = SqliteCacheMode.Shared,
                }.ToString());
            }
        }
    }
}
