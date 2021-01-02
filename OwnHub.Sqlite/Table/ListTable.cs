using System.Threading.Tasks;
using BitFaster.Caching.Lru;
using Microsoft.Data.Sqlite;

namespace OwnHub.Sqlite.Table
{
    /// <summary>
    /// The list stored by sqlite has a column of auto-increment id and a column of data.
    /// </summary>
    public class ListTable
    {
        private string DatabaseDropCommand => $@"
            DROP TABLE IF EXISTS {tableName};
            ";

        private string DatabaseCreateCommand => $@"
            CREATE TABLE IF NOT EXISTS {tableName} (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                obj TEXT
            );
            ";
        
        private readonly SqliteContext context;
        private readonly string tableName;
        private ConcurrentLru<long, string> lru = new ConcurrentLru<long, string>(1000);
        
        public ListTable(SqliteContext context, string tableName)
        {
            this.context = context;
            this.tableName = tableName;
        }

        public Task Create()
        {
            return context.Create(async (connection) =>
            {
                SqliteCommand command = connection.CreateCommand();
                command.CommandText = DatabaseCreateCommand;
                await command.ExecuteNonQueryAsync();
            });
        }

        public Task Drop()
        {
            return context.Write(async (connection) =>
            {
                SqliteCommand command = connection.CreateCommand();
                command.CommandText = DatabaseDropCommand;
                await command.ExecuteNonQueryAsync();
            });
        }

        public Task<long> Insert(string obj)
        {
            return context.Write(async (connection) =>
            {
                SqliteCommand command = connection.CreateCommand();
                command.CommandText = $@"
                INSERT INTO {tableName} (obj) VALUES (
                    $obj
                );
                SELECT last_insert_rowid();
                ";;
                command.Parameters.AddWithValue("$obj", obj);
                var id = (long) await command.ExecuteScalarAsync();

                lru.GetOrAdd(id, _ => obj);
                return id;
            });
        }

        public Task<string?> Search(long id)
        {
            if (lru.TryGet(id, out string cachedObj))
            {
                return Task.FromResult(cachedObj)!;
            }
            return context.Read(async (connection) =>
            {
                SqliteCommand command = connection.CreateCommand();
                command.CommandText = $@"
                SELECT obj FROM {tableName}
                    WHERE id=$id
                ";
                command.Parameters.AddWithValue("$id", id);
                SqliteDataReader reader = await command.ExecuteReaderAsync();
                
                if (reader.Read())
                {
                    string obj = reader.GetString(0);

                    lru.GetOrAdd(id, _ => obj);
                    return obj;
                }

                return null;
            });
        }
    }
}