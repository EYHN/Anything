using System.Linq;
using System.Threading.Tasks;
using Anything.Search;
using Anything.Search.Indexers;
using Anything.Search.Properties;
using Anything.Search.Query;
using Anything.Utils;
using NUnit.Framework;

namespace Anything.Tests.Search.Indexers
{
    public class IndexerTests
    {
        [Test]
        public async Task LuceneIndexerTest()
        {
            using var index = new LuceneIndexer(TestUtils.GetTestDirectoryPath());
            await TestIndexer(index);
        }

        private async Task TestIndexer(ISearchIndexer indexer)
        {
            static SearchPropertyValueSet PropertyValues(params SearchPropertyValue[] values)
            {
                return new(values);
            }

            await indexer.BatchIndex(
                new[]
                {
                    (Url.Parse("file://test/foobar"),
                        PropertyValues(new SearchPropertyValue(SearchProperty.FileName, "foobar"))),
                    (Url.Parse("file://test/foo"),
                        PropertyValues(new SearchPropertyValue(SearchProperty.FileName, "foo"))),
                    (Url.Parse("file://test/foo/bar"),
                        PropertyValues(new SearchPropertyValue(SearchProperty.FileName, "bar"))),
                    (Url.Parse("file://test/foo/foobar"),
                        PropertyValues(new SearchPropertyValue(SearchProperty.FileName, "foobar")))
                });

            await indexer.ForceRefresh();
            {
                var result = await indexer.Search(
                    new SearchOptions(new TextSearchQuery(SearchProperty.FileName, "bar")));

                Assert.AreEqual(3, result.Nodes.Length);
                Assert.True(result.Nodes.Any(node => node.Url == Url.Parse("file://test/foobar")));
                Assert.True(result.Nodes.Any(node => node.Url == Url.Parse("file://test/foo/bar")));
                Assert.True(result.Nodes.Any(node => node.Url == Url.Parse("file://test/foo/foobar")));
            }

            {
                var result = await indexer.Search(
                    new SearchOptions(new TextSearchQuery(SearchProperty.FileName, "foo")));

                Assert.AreEqual(3, result.Nodes.Length);
                Assert.True(result.Nodes.Any(node => node.Url == Url.Parse("file://test/foobar")));
                Assert.True(result.Nodes.Any(node => node.Url == Url.Parse("file://test/foo")));
                Assert.True(result.Nodes.Any(node => node.Url == Url.Parse("file://test/foo/foobar")));
            }

            {
                var result = await indexer.Search(
                    new SearchOptions(new TextSearchQuery(SearchProperty.FileName, "bar"), Url.Parse("file://test/foo")));

                Assert.AreEqual(2, result.Nodes.Length);
                Assert.True(result.Nodes.Any(node => node.Url == Url.Parse("file://test/foo/bar")));
                Assert.True(result.Nodes.Any(node => node.Url == Url.Parse("file://test/foo/foobar")));
            }

            await indexer.BatchDelete(new[] { Url.Parse("file://test/foo/bar") });
            await indexer.ForceRefresh();
            {
                var result = await indexer.Search(
                    new SearchOptions(new TextSearchQuery(SearchProperty.FileName, "bar")));

                Assert.AreEqual(2, result.Nodes.Length);
                Assert.True(result.Nodes.Any(node => node.Url == Url.Parse("file://test/foobar")));
                Assert.True(result.Nodes.Any(node => node.Url == Url.Parse("file://test/foo/foobar")));
            }

            {
                var result1 = await indexer.Search(
                    new SearchOptions(
                        new TextSearchQuery(SearchProperty.FileName, "foo"),
                        null,
                        new SearchPagination(2)));
                Assert.AreEqual(2, result1.Nodes.Length);

                await indexer.BatchDelete(new[]
                {
                    Url.Parse("file://test/foobar"), Url.Parse("file://test/foo"), Url.Parse("file://test/foo/foobar")
                });

                var result2 = await indexer.Search(
                    new SearchOptions(
                        new TextSearchQuery(SearchProperty.FileName, "foo"),
                        null,
                        new SearchPagination(2, result1.Nodes[1].Cursor, result1.PageInfo.ScrollId)));
                Assert.AreEqual(1, result2.Nodes.Length);

                var result = result1.Nodes.Concat(result2.Nodes).ToArray();
                Assert.AreEqual(3, result.Length);
                Assert.True(result.Any(node => node.Url == Url.Parse("file://test/foobar")));
                Assert.True(result.Any(node => node.Url == Url.Parse("file://test/foo")));
                Assert.True(result.Any(node => node.Url == Url.Parse("file://test/foo/foobar")));
            }
        }
    }
}
