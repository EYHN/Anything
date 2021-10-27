using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
using Anything.FileSystem.Impl;
using Anything.Search;
using Anything.Search.Crawlers;
using Anything.Search.Indexers;
using Anything.Search.Properties;
using Anything.Search.Query;
using Anything.Utils;
using Moq;
using NUnit.Framework;

namespace Anything.Tests.Search
{
    public class SearchServiceTests
    {
        [Test]
        public async Task FeatureTest()
        {
            using var fileService = new FileService();
            fileService.AddFileSystem("test", new MemoryFileSystem());
            var mockIndexer = new Mock<ISearchIndexer>();
            var mockCrawler = new Mock<ISearchCrawler>();
            using var searchService = new SearchService(fileService, mockIndexer.Object, new[] { mockCrawler.Object });
            {
                // test auto index
                var testPropertyValueSet = new SearchPropertyValueSet(new SearchPropertyValue[] { new(SearchProperty.FileName, "foo") });

                mockCrawler.Setup(crawler => crawler.GetData(It.IsAny<FileHandle>()))
                    .Returns(() => ValueTask.FromResult(testPropertyValueSet));

                mockIndexer.Setup(indexer =>
                        indexer.BatchIndex(It.IsAny<(Url Url, FileHandle FileHandle, SearchPropertyValueSet Properties)[]>()))
                    .Returns(ValueTask.CompletedTask);

                var root = await fileService.CreateFileHandle(Url.Parse("file://test/"));
                var testDir = await fileService.CreateDirectory(root, "foo");
                var testDirUrl = await fileService.GetUrl(testDir);
                await fileService.WaitComplete();

                mockCrawler.Verify(crawler => crawler.GetData(It.Is<FileHandle>(fileHandle => fileHandle == testDir)), Times.Once);
                mockIndexer.Verify(
                    indexer => indexer.BatchIndex(
                        It.Is<(Url Url, FileHandle FileHandle, SearchPropertyValueSet Properties)[]>(
                            payload =>
                                payload.Length == 1 &&
                                payload.Any(file =>
                                    file.Url == testDirUrl && file.FileHandle == testDir && file.Properties == testPropertyValueSet))),
                    Times.Once);

                // test query
                var testQuery = new TextSearchQuery(SearchProperty.FileName, "foo");
                var testQueryPagination = new SearchPagination(20, null, null);
                var testQueryBaseUrl = Url.Parse("file://test/");
                var testQueryOptions = new SearchOptions(testQuery, testQueryBaseUrl, testQueryPagination);

                mockIndexer.Setup(indexer => indexer.Search(It.IsAny<SearchOptions>()))
                    .Returns(
                        () => ValueTask.FromResult(
                            new SearchResult(new[] { new SearchResultNode(testDir, "1") }, new SearchPageInfo(1, null))));

                var result = await searchService.Search(testQueryOptions);
                Assert.IsTrue(result.Nodes.Contains(new SearchResultNode(testDir, "1")));

                mockIndexer.Verify(indexer => indexer.Search(It.Is<SearchOptions>(options => options == testQueryOptions)));

                mockCrawler.VerifyNoOtherCalls();
                mockIndexer.VerifyNoOtherCalls();
            }
        }
    }
}
