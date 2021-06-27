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
            static SearchPropertyValueSet PropertyValues(params (SearchProperty Property, object Data)[] values)
            {
                return new(values);
            }

            await indexer.Index(
                Url.Parse("file://test/foobar"),
                PropertyValues((SearchProperty.FileName, "foobar")));

            await indexer.Index(
                Url.Parse("file://test/foo"),
                PropertyValues((SearchProperty.FileName, "foo")));

            await indexer.Index(
                Url.Parse("file://test/foo/bar"),
                PropertyValues((SearchProperty.FileName, "bar")));
            {
                var result = await indexer.Search(
                    new SearchOptions(new TextSearchQuery(SearchProperty.FileName, "bar")));

                Assert.AreEqual(2, result.Items.Length);
                Assert.True(result.Items.Contains(Url.Parse("file://test/foobar")));
                Assert.True(result.Items.Contains(Url.Parse("file://test/foo/bar")));
            }

            {
                var result = await indexer.Search(
                    new SearchOptions(new TextSearchQuery(SearchProperty.FileName, "foo")));

                Assert.AreEqual(2, result.Items.Length);
                Assert.True(result.Items.Contains(Url.Parse("file://test/foobar")));
                Assert.True(result.Items.Contains(Url.Parse("file://test/foo")));
            }

            {
                var result = await indexer.Search(
                    new SearchOptions(new TextSearchQuery(SearchProperty.FileName, "bar"), Url.Parse("file://test/foo")));

                Assert.AreEqual(1, result.Items.Length);
                Assert.True(result.Items.Contains(Url.Parse("file://test/foo/bar")));
            }

            await indexer.Delete(Url.Parse("file://test/foo/bar"));
            {
                var result = await indexer.Search(
                    new SearchOptions(new TextSearchQuery(SearchProperty.FileName, "bar")));

                Assert.AreEqual(1, result.Items.Length);
                Assert.True(result.Items.Contains(Url.Parse("file://test/foobar")));
            }
        }
    }
}
