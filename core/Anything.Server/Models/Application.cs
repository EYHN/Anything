using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Notes;
using Anything.Preview;
using Anything.Search;
using Anything.Search.Properties;
using Anything.Search.Query;
using Anything.Tags;
using Anything.Utils;
using Microsoft.Extensions.Configuration;

namespace Anything.Server.Models
{
    public class Application
    {
        public Application(
            IConfiguration configuration,
            IFileService fileService,
            IPreviewService previewService,
            ISearchService searchService,
            ITagService tagService,
            INoteService noteService)
        {
            Configuration = configuration;
            FileService = fileService;
            PreviewService = previewService;
            SearchService = searchService;
            TagService = tagService;
            NoteService = noteService;
        }

        public IConfiguration Configuration { get; }

        public IFileService FileService { get; }

        public IPreviewService PreviewService { get; }

        public ISearchService SearchService { get; }

        public ITagService TagService { get; }

        public INoteService NoteService { get; }

        public async ValueTask<FileHandleRef> CreateFileHandle(Url url)
        {
            var fileHandle = await FileService.CreateFileHandle(url);
            return new FileHandleRef(this, fileHandle);
        }

        public ValueTask<FileHandleRef> OpenFileHandle(FileHandle fileHandle)
        {
            return ValueTask.FromResult(new FileHandleRef(this, fileHandle));
        }

        public async ValueTask<File> AddTags(FileHandle fileHandle, Tag[] tags)
        {
            await TagService.AddTags(fileHandle, tags);
            return CreateFile(fileHandle, await FileService.Stat(fileHandle));
        }

        public async ValueTask<File> RemoveTags(FileHandle fileHandle, Tag[] tags)
        {
            await TagService.RemoveTags(fileHandle, tags);
            return CreateFile(fileHandle, await FileService.Stat(fileHandle));
        }

        public async ValueTask<File> SetNote(FileHandle fileHandle, string note)
        {
            await NoteService.SetNote(fileHandle, note);
            return CreateFile(fileHandle, await FileService.Stat(fileHandle));
        }

        internal File CreateFile(FileHandle fileHandle, FileStats stats)
        {
            if (stats.Type.HasFlag(FileType.File))
            {
                return new RegularFile(this, fileHandle, stats);
            }

            if (stats.Type.HasFlag(FileType.Directory))
            {
                return new Directory(this, fileHandle, stats);
            }

            return new UnknownFile(this, fileHandle, stats);
        }

        public async ValueTask<File[]> Search(string q, Url? baseUrl)
        {
            var result = await SearchService.Search(new SearchOptions(new TextSearchQuery(SearchProperty.FileName, q), baseUrl));
            return await Task.WhenAll(
                result.Nodes.Select(async node => CreateFile(node.FileHandle, await FileService.Stat(node.FileHandle))));
        }
    }
}
