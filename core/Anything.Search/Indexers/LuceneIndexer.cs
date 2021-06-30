using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
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
using Nito.AsyncEx;
using Directory = Lucene.Net.Store.Directory;

namespace Anything.Search.Indexers
{
    public class LuceneIndexer : ISearchIndexer, IDisposable
    {
        private bool _disposed;
        private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
        private const string UrlFieldKey = "_url";
        private readonly Analyzer _analyzer;
        private readonly Directory _directory;
        private readonly AsyncLock _writeLock = new();
        private readonly IndexWriter _writer;
        private readonly SearcherLifetimeManager _lifetimeManager;
        private readonly SearcherManager _searcherManager;
        private readonly CancellationTokenSource _refreshLoopCancellationTokenSource = new();

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

            var indexConfig = new IndexWriterConfig(AppLuceneVersion, _analyzer);
            _writer = new IndexWriter(_directory, indexConfig);
            _writer.Commit();

            _searcherManager = new SearcherManager(_directory, null);
            _lifetimeManager = new SearcherLifetimeManager();

            Task.Run(async () =>
            {
                while (!_refreshLoopCancellationTokenSource.IsCancellationRequested)
                {
                    _searcherManager.MaybeRefresh();
                    _lifetimeManager.Prune(new SearcherLifetimeManager.PruneByAge(60 * 10));
                    await Task.Delay(1000);
                }
            });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _writer.Dispose();
                    _lifetimeManager.Dispose();
                    _searcherManager.Dispose();
                    _directory.Dispose();
                    _refreshLoopCancellationTokenSource.Cancel();
                }

                _disposed = true;
            }
        }

        ~LuceneIndexer()
        {
            Dispose(false);
        }

        public Task BatchIndex((Url Url, SearchPropertyValueSet Properties)[] payload)
        {
            using (_writeLock.Lock())
            {
                try
                {
                    foreach (var (url, valueSet) in payload)
                    {
                        _writer.DeleteDocuments(new Term(UrlFieldKey, url.ToString()));

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

                        _writer.AddDocument(doc);
                    }

                    _writer.Commit();
                    return Task.CompletedTask;
                }
                catch
                {
                    _writer.Rollback();
                    throw;
                }
            }
        }

        public Task BatchDelete(Url[] urls)
        {
            using (_writeLock.Lock())
            {
                try
                {
                    foreach (var url in urls)
                    {
                        _writer.DeleteDocuments(new Term(UrlFieldKey, url.ToString()));
                    }

                    _writer.Commit();
                    return Task.CompletedTask;
                }
                catch
                {
                    _writer.Rollback();
                    throw;
                }
            }
        }

        public Task<SearchResult> Search(SearchOptions options)
        {
            IndexSearcher searcher;
            var scrollId = options.Pagination.ScrollId;
            if (scrollId != null)
            {
                searcher = _lifetimeManager.Acquire(DeserializeScrollId(scrollId));
                if (searcher == null)
                {
                    throw new ScrollIdNotFoundException();
                }
            }
            else
            {
                searcher = _searcherManager.Acquire();
                var token = _lifetimeManager.Record(searcher);
                scrollId = SerializeScrollId(token);
            }

            var query = new BooleanQuery();

            if (options.BaseUrl != null)
            {
                var startsWith = options.BaseUrl.ToString() + '/';
                var endsWith = options.BaseUrl.ToString() + '0'; // '0' is next char character of '/' in ascii
                query.Add(TermRangeQuery.NewStringRange(UrlFieldKey, startsWith, endsWith, false, false), Occur.MUST);
            }

            query.Add(ConvertSearchQuery(options.Query), Occur.MUST);

            var pagination = options.Pagination;

            TopDocs topDocs;

            if (pagination.After == null)
            {
                topDocs = searcher.Search(query, pagination.Size);
            }
            else
            {
                var after = DeserializeCursor(pagination.After);
                topDocs = searcher.SearchAfter(after, query, pagination.Size);
            }

            return Task.FromResult(
                new SearchResult(
                    topDocs.ScoreDocs
                        .Select(scoreDoc =>
                            new SearchResultNode(Url.Parse(searcher.Doc(scoreDoc.Doc).Get(UrlFieldKey)), SerializeCursor(scoreDoc)))
                        .ToArray(),
                    new SearchPageInfo(topDocs.TotalHits, scrollId)));
        }

        public Task ForceRefresh()
        {
            _searcherManager.MaybeRefreshBlocking();
            return Task.CompletedTask;
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

        private string SerializeCursor(ScoreDoc scoreDoc)
        {
            using var memoryStream = new MemoryStream(4 * 3);
            var binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write(scoreDoc.Doc);
            binaryWriter.Write(scoreDoc.Score);
            binaryWriter.Write(scoreDoc.ShardIndex);
            return Convert.ToBase64String(memoryStream.ToArray());
        }

        private ScoreDoc DeserializeCursor(string cursor)
        {
            using var memoryStream = new MemoryStream(Convert.FromBase64String(cursor));
            var binaryReader = new BinaryReader(memoryStream);
            var doc = binaryReader.ReadInt32();
            var score = binaryReader.ReadSingle();
            var sharedIndex = binaryReader.ReadInt32();
            return new ScoreDoc(doc, score, sharedIndex);
        }

        private string SerializeScrollId(long scrollId)
        {
            using var memoryStream = new MemoryStream(8);
            var binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write(scrollId);
            return Convert.ToBase64String(memoryStream.ToArray());
        }

        private long DeserializeScrollId(string scrollId)
        {
            using var memoryStream = new MemoryStream(Convert.FromBase64String(scrollId));
            var binaryReader = new BinaryReader(memoryStream);
            return binaryReader.ReadInt64();
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
