using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Anything.Database;
using Anything.Database.Table;
using Anything.FileSystem;
using Anything.FileSystem.Tracker;
using Anything.Utils;
using Dynamitey.DynamicObjects;
using IDbTransaction = Anything.Database.IDbTransaction;

namespace Anything.Search.Modules.FileName
{
    public class FileNameIndexer
    {
        private readonly FileNameTable _fileNameTable;
        private readonly SqliteContext _sqliteContext;

        public FileNameIndexer(SqliteContext sqliteContext)
        {
            _sqliteContext = sqliteContext;
            _fileNameTable = new FileNameTable("FileNames");

            using var transaction = new SqliteTransaction(_sqliteContext, ITransaction.TransactionMode.Create);
            _fileNameTable.Create(transaction);
            transaction.Commit();
        }

        public void BindingAutoIndex(IFileService fileService)
        {
            fileService.FileTracker.OnFileChange.On(async events =>
            {
                var indexList = new List<Url>();
                var deleteList = new List<Url>();
                foreach (var @event in events)
                {
                    if (@event.Type is FileChangeEvent.EventType.Created)
                    {
                        indexList.Add(@event.Url);
                    }

                    if (@event.Type is FileChangeEvent.EventType.Deleted)
                    {
                        deleteList.Add(@event.Url);
                    }
                }

                await BatchDelete(deleteList.ToArray());
                await BatchIndex(indexList.Select(url => (url.Basename(), url)).ToArray());
            });
        }

        public async ValueTask Index(string fileName, Url url)
        {
            await using var transaction = new SqliteTransaction(_sqliteContext, ITransaction.TransactionMode.Mutation);
            await _fileNameTable.InsertAsync(transaction, fileName, url);
            await transaction.CommitAsync();
        }

        public async ValueTask BatchIndex((string FileName, Url Url)[] items)
        {
            await using var transaction = new SqliteTransaction(_sqliteContext, ITransaction.TransactionMode.Mutation);
            foreach (var (fileName, url) in items)
            {
                await _fileNameTable.InsertAsync(transaction, fileName, url);
            }

            await transaction.CommitAsync();
        }

        public async ValueTask Delete(Url url)
        {
            await using var transaction = new SqliteTransaction(_sqliteContext, ITransaction.TransactionMode.Mutation);
            await _fileNameTable.DeleteAsync(transaction, url);
            await transaction.CommitAsync();
        }

        public async ValueTask BatchDelete(Url[] urls)
        {
            await using var transaction = new SqliteTransaction(_sqliteContext, ITransaction.TransactionMode.Mutation);
            foreach (var url in urls)
            {
                await _fileNameTable.DeleteAsync(transaction, url);
            }

            await transaction.CommitAsync();
        }

        public async ValueTask<Url[]> Search(string searchString, Url? baseUrl)
        {
            await using var transaction = new SqliteTransaction(_sqliteContext, ITransaction.TransactionMode.Query);
            return await _fileNameTable.SearchAsync(transaction, searchString, baseUrl);
        }

        public class FileNameTable : Table
        {
            public FileNameTable(string tableName)
                : base(tableName)
            {
            }

            protected override string DatabaseCreateCommand => $@"
                CREATE VIRTUAL TABLE IF NOT EXISTS {TableName}Texts USING fts5(Text, tokenize=""trigram"");
                CREATE TABLE IF NOT EXISTS {TableName} (Url TEXT NOT NULL UNIQUE, TextId INTEGER NOT NULL UNIQUE);
                ";

            protected override string DatabaseDropCommand => $@"
                DROP TABLE IF EXISTS {TableName};
                DROP TABLE IF EXISTS {TableName}Texts;
                ";

            private string InsertCommand => $@"
                INSERT INTO {TableName}Texts VALUES (@FileName);
                INSERT INTO {TableName} (Url, TextId) VALUES (@Url, last_insert_rowid());
                ";

            private string SearchCommand => $@"
                SELECT {TableName}.Url
                FROM {TableName}
                JOIN {TableName}Texts ON {TableName}Texts.rowid = {TableName}.TextId
                WHERE {TableName}.Url LIKE @UrlLike ESCAPE '\' AND {TableName}Texts.Text MATCH @SearchString
                ORDER BY {TableName}Texts.rank;
                ";

            private string SearchWithoutBaseUrlCommand => $@"
                SELECT {TableName}.Url
                FROM {TableName}
                JOIN {TableName}Texts ON {TableName}Texts.rowid = {TableName}.TextId
                WHERE  {TableName}Texts.Text MATCH @SearchString
                ORDER BY {TableName}Texts.rank;
                ";

            private string DeleteCommand => $@"
                DELETE FROM {TableName}Texts WHERE rowid IN (SELECT TextId FROM {TableName} WHERE Url = @Url);
                DELETE FROM {TableName} WHERE Url = @Url;
                ";

            public async Task InsertAsync(IDbTransaction transaction, string fileName, Url url)
            {
                await transaction.ExecuteNonQueryAsync(
                    () => InsertCommand,
                    $"{nameof(FileNameIndexer)}/{nameof(InsertCommand)}/{TableName}",
                    new Dictionary { { "@Url", url.ToString() }, { "@FileName", fileName } });
            }

            public async Task<Url[]> SearchAsync(IDbTransaction transaction, string searchString, Url? baseUrl)
            {
                if (baseUrl != null)
                {
                    return await transaction.ExecuteReaderAsync(
                        () => SearchCommand,
                        $"{nameof(FileNameIndexer)}/{nameof(SearchCommand)}/{TableName}",
                        HandleReadSearchResult,
                        new Dictionary { { "@UrlLike", BaseUrlToLike(baseUrl) }, { "@SearchString", searchString } });
                }

                return await transaction.ExecuteReaderAsync(
                    () => SearchWithoutBaseUrlCommand,
                    $"{nameof(FileNameIndexer)}/{nameof(SearchWithoutBaseUrlCommand)}/{TableName}",
                    HandleReadSearchResult,
                    new Dictionary { { "@SearchString", searchString } });
            }

            public async Task DeleteAsync(IDbTransaction transaction, Url url)
            {
                await transaction.ExecuteNonQueryAsync(
                    () => DeleteCommand,
                    $"{nameof(FileNameIndexer)}/{nameof(DeleteCommand)}/{TableName}",
                    new Dictionary { { "@Url", url.ToString() } });
            }

            private Url[] HandleReadSearchResult(IDataReader reader)
            {
                var result = new List<Url>();
                while (reader.Read())
                {
                    result.Add(Url.Parse(reader.GetString(0)));
                }

                return result.ToArray();
            }

            private string BaseUrlToLike(Url baseUrl)
            {
                var url = baseUrl.ToString().Replace("%", "\\%").Replace("_", "\\_");
                if (url.EndsWith("/"))
                {
                    return url + "%";
                }

                return url + "/%";
            }
        }
    }
}
