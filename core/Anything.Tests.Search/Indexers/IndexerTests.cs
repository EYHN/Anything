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
            await TestIndexer(new LuceneIndexer(TestUtils.GetTestDirectoryPath()));
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
            {
                var result = await indexer.Search(
                    new SearchOptions(new TextSearchQuery(SearchProperty.FileName, "bar")));

                Assert.AreEqual(2, result.Nodes.Length);
                Assert.True(result.Nodes.Any(node => node.Url == Url.Parse("file://test/foobar")));
                Assert.True(result.Nodes.Any(node => node.Url == Url.Parse("file://test/foo/foobar")));
            }
        }
    }
}
