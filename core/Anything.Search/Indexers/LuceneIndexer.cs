using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Anything.Search.Exception;
using Anything.Search.Properties;
using Anything.Search.Query;
using Anything.Utils;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.NGram;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Flexible.Standard;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Directory = Lucene.Net.Store.Directory;

namespace Anything.Search.Indexers
{
    public class LuceneIndexer : ISearchIndexer, IDisposable
    {
        private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
        private const string UrlFieldKey = "_url";
        private readonly Analyzer _analyzer;
        private readonly Directory _directory;

        public LuceneIndexer(string indexPath)
        {
            _directory = FSDirectory.Open(indexPath);

            var propertyTypes = typeof(SearchProperty).GetProperties(BindingFlags.Static | BindingFlags.Public)
                .Where(info => info.PropertyType == typeof(SearchProperty)).Select(info => (SearchProperty)info.GetValue(null)!);

            var fieldAnalyzers = propertyTypes
                .ToDictionary<SearchProperty, string, Analyzer>(property => property.Name, property => property.Type switch
                {
                    SearchProperty.DataType.Text => new NgramAnalyzer(AppLuceneVersion),
                    _ => throw new ArgumentOutOfRangeException()
                });

            _analyzer = new PerFieldAnalyzerWrapper(
                new StandardAnalyzer(AppLuceneVersion),
                fieldAnalyzers);
        }

        public void Dispose()
        {
            _directory.Dispose();
        }

        public Task Index(Url url, SearchPropertyValueSet valueSet)
        {
            return BatchIndex(new[] { (url, valueSet) });
        }

        public Task BatchIndex((Url Url, SearchPropertyValueSet Properties)[] payload)
        {
            var indexConfig = new IndexWriterConfig(AppLuceneVersion, _analyzer);
            using var writer = new IndexWriter(_directory, indexConfig);

            foreach (var (url, valueSet) in payload)
            {
                writer.DeleteDocuments(new Term(UrlFieldKey, url.ToString()));

                var doc = new Document { new StringField(UrlFieldKey, url.ToString(), Field.Store.YES) };
                foreach (var item in valueSet)
                {
                    Field field = item.Property.Type switch
                    {
                        SearchProperty.DataType.Text => new TextField(item.Property.Name, (string)item.Data, Field.Store.YES),
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    doc.Add(field);
                }

                writer.AddDocument(doc);
            }

            writer.Commit();
            return Task.CompletedTask;
        }

        public Task Delete(Url url)
        {
            return BatchDelete(new[] { url });
        }

        public Task BatchDelete(Url[] urls)
        {
            var indexConfig = new IndexWriterConfig(AppLuceneVersion, _analyzer);
            using var writer = new IndexWriter(_directory, indexConfig);

            foreach (var url in urls)
            {
                writer.DeleteDocuments(new Term(UrlFieldKey, url.ToString()));
            }

            writer.Commit();
            return Task.CompletedTask;
        }

        public Task<SearchResult> Search(SearchOptions options)
        {
            using var reader = DirectoryReader.Open(_directory);
            var searcher = new IndexSearcher(reader);

            var query = new BooleanQuery();

            if (options.BaseUrl != null)
            {
                var startsWith = options.BaseUrl.ToString() + '/';
                var endsWith = options.BaseUrl.ToString() + '0'; // '0' is next char character of '/' in ascii
                query.Add(TermRangeQuery.NewStringRange(UrlFieldKey, startsWith, endsWith, false, false), Occur.MUST);
            }

            query.Add(ConvertSearchQuery(options.Query), Occur.MUST);

            var pagination = options.Pagination;

            if (pagination.After == null)
            {
                var topDocs = searcher.Search(query, pagination.First);
                return Task.FromResult(
                    new SearchResult(
                        topDocs.ScoreDocs
                            .Select(scoreDoc => Url.Parse(searcher.Doc(scoreDoc.Doc).Get(UrlFieldKey)))
                            .ToArray()));
            }

            var afterSearchTopDocs = searcher.Search(
                new TermQuery(new Term(UrlFieldKey, pagination.After.ToString())), 1);
            if (afterSearchTopDocs.TotalHits == 0)
            {
                throw new AfterNotFoundException();
            }

            var afterDoc = afterSearchTopDocs.ScoreDocs[0];

            return Task.FromResult(
                new SearchResult(
                    searcher.SearchAfter(afterDoc, query, pagination.First).ScoreDocs
                        .Select(scoreDoc => Url.Parse(searcher.Doc(scoreDoc.Doc).Get(UrlFieldKey)))
                        .ToArray()));
        }

        private Lucene.Net.Search.Query ConvertSearchQuery(SearchQuery searchQuery)
        {
            if (searchQuery is BooleanSearchQuery booleanSearchQuery)
            {
                var targetQuery = new BooleanQuery();
                foreach (var clause in booleanSearchQuery)
                {
                    targetQuery.Add(ConvertSearchQuery(clause.Query), CovertOccur(clause.Occur));
                }

                return targetQuery;
            }

            if (searchQuery is TextSearchQuery textSearchQuery)
            {
                StandardQueryParser queryParser = new() { Analyzer = _analyzer };

                var filteredSearchString = new Regex(@"(?<!\\):").Replace(textSearchQuery.Text, "\\:");
                return queryParser.Parse(filteredSearchString, textSearchQuery.Property.Name);
            }

            throw new ArgumentOutOfRangeException(nameof(searchQuery), searchQuery, "Unknown query type.");
        }

        private Occur CovertOccur(BooleanSearchQuery.Occur occur)
        {
            return occur switch
            {
                BooleanSearchQuery.Occur.Must => Occur.MUST,
                BooleanSearchQuery.Occur.Should => Occur.SHOULD,
                BooleanSearchQuery.Occur.MustNot => Occur.MUST_NOT,
                _ => throw new ArgumentOutOfRangeException(nameof(occur), occur, null)
            };
        }

        private class NgramAnalyzer : Analyzer
        {
            private readonly LuceneVersion _matchVersion;

            public NgramAnalyzer(LuceneVersion matchVersion)
            {
                _matchVersion = matchVersion;
            }

            protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
            {
                var source = new NGramTokenizer(_matchVersion, reader);
                var filter = new LowerCaseFilter(_matchVersion, source);
                return new TokenStreamComponents(source, filter);
            }
        }
    }
}
