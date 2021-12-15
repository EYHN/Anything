using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.Search.Properties;

namespace Anything.Search.Crawlers;

public class FileNameSearchCrawler : ISearchCrawler
{
    private readonly IFileService _fileService;

    public FileNameSearchCrawler(IFileService fileService)
    {
        _fileService = fileService;
    }

    public async ValueTask<SearchPropertyValueSet> GetData(FileHandle fileHandle)
    {
        var filename = await _fileService.GetFileName(fileHandle);
        return new SearchPropertyValueSet(new[] { new SearchPropertyValue(SearchProperty.FileName, filename) });
    }
}
