using System.Linq;
using System.Threading.Tasks;
using Anything.FileSystem;
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
            var fileService = FileServiceFactory.BuildMemoryFileService(Url.Parse("file://test/"));
            var mockIndexer = new Mock<ISearchIndexer>();
            var mockCrawler = new Mock<ISearchCrawler>();
            var searchService = new SearchService(fileService, mockIndexer.Object, new[] { mockCrawler.Object });
            {
                // test auto index
                var testUrl = Url.Parse("file://test/foo");
                var testPropertyValueSet = new SearchPropertyValueSet(new SearchPropertyValue[] { new(SearchProperty.FileName, "foo") });

                mockCrawler.Setup(crawler => crawler.GetData(It.IsAny<Url>()))
                    .Returns(Task.FromResult(testPropertyValueSet));

                mockIndexer.Setup(indexer => indexer.BatchIndex(It.IsAny<(Url Url, SearchPropertyValueSet Properties)[]>()))
                    .Returns(Task.CompletedTask);

                await fileService.CreateDirectory(testUrl); // should auto index new directory

                await fileService.WaitComplete();

                mockCrawler.Verify(crawler => crawler.GetData(It.Is<Url>(url => url == testUrl)), Times.Once);
                mockIndexer.Verify(
                    indexer => indexer.BatchIndex(
                        It.Is<(Url Url, SearchPropertyValueSet Properties)[]>(
                            payload =>
                                payload.Length == 1 &&
                                payload.Any(file => file.Url == testUrl && file.Properties == testPropertyValueSet))),
                    Times.Once);

                // test query
                var testQuery = new TextSearchQuery(SearchProperty.FileName, "foo");
                var testQueryPagination = new SearchPagination(20);
                var testQueryBaseUrl = Url.Parse("file://test/");
                var testQueryOptions = new SearchOptions(testQuery, testQueryBaseUrl, testQueryPagination);

                mockIndexer.Setup(indexer => indexer.Search(It.IsAny<SearchOptions>()))
                    .Returns(
                        Task.FromResult(
                            new SearchResult(new[] { new SearchResultNode(testUrl, "1") }, new SearchPageInfo(1))));

                var result = await searchService.Search(testQueryOptions);
                Assert.Contains(new SearchResultNode(testUrl, "1"), result.Nodes);

                mockIndexer.Verify(indexer => indexer.Search(It.Is<SearchOptions>(options => options == testQueryOptions)));

                mockCrawler.VerifyNoOtherCalls();
                mockIndexer.VerifyNoOtherCalls();
            }
        }
    }
}
