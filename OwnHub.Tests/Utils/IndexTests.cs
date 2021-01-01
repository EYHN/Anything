using System;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.Data.Sqlite;
using NUnit.Framework;

namespace OwnHub.Tests.Utils
{
    [TestFixture]
    public class IndexTests
    {
        [Test]
        [Timeout(3000)]
        public void IndexTest()
        {
            SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder();
            builder.Mode = SqliteOpenMode.Memory;
            SqliteConnection connection = new SqliteConnection(builder.ConnectionString);
            connection.Open();
            SqliteCommand sqliteCommand = connection.CreateCommand();
            sqliteCommand.CommandText = "PRAGMA module_list;";
            SqliteDataReader reader = sqliteCommand.ExecuteReader();
            while (reader.Read())
            {
                var name = reader.GetString(0);

                Console.WriteLine($"{name}");
            }
            // using var dir = new RAMDirectory();
            //
            // // Create an analyzer to process the text
            // var analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);
            //
            // // Create an index writer
            // var indexConfig = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer);
            // using var writer = new IndexWriter(dir, indexConfig);
            //
            // var doc = new Document
            // {
            //     // StringField indexes but doesn't tokenize
            //     new StringField("path", 
            //         "/Test/metadata.jpg", 
            //         Field.Store.YES),
            //     new StoredField("etag", "123"),
            //     new StringField("metadata:Camera.Make", "SONY", Field.Store.NO),
            //     new Int32Field("metadata:Palette", 0x806840, Field.Store.NO)
            //     {
            //         Boost = 2f
            //     },
            //     new Int32Field("metadata:Palette", 0x585040, Field.Store.NO),
            //     new Int32Field("metadata:Palette", 0x988860, Field.Store.NO),
            //     new Int32Field("metadata:Palette", 0xB0A078, Field.Store.NO),
            //     new Int32Field("metadata:Palette", 0x584820, Field.Store.NO),
            // };
            //
            // writer.AddDocument(doc);
            // writer.Flush(triggerMerge: false, applyAllDeletes: false);
            //
            // var phrase = new BooleanQuery();
            // {
            // };
            //
            //
            // using var reader = writer.GetReader(applyAllDeletes: true);
            // var searcher = new IndexSearcher(reader);
            // var hits = searcher.Search(phrase, 20 /* top 20 */).ScoreDocs;
            //
            // foreach (var hit in hits)
            // {
            //     var foundDoc = searcher.Doc(hit.Doc);
            //     Console.WriteLine($"{hit.Score}" +
            //                       $" {foundDoc.Get("path")}" +
            //                       $" {foundDoc.Get("etag")}" +
            //                       $" {foundDoc.Get("metadata:Camera.Make")}");
            // }
        }
    }
}