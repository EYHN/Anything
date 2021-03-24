using System.Threading.Tasks;
using BitFaster.Caching.Lru;

namespace OwnHub.Sqlite.Table
{
    /// <summary>
    /// The list stored by sqlite has a column of auto-increment id and a column of data.
    /// </summary>
    public class ListTable
    {
        private string DatabaseDropCommand => $@"
            DROP TABLE IF EXISTS {_tableName};
            ";

        private string DatabaseCreateCommand => $@"
            CREATE TABLE IF NOT EXISTS {_tableName} (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                obj TEXT
            );
            ";

        private readonly SqliteContext _context;
        private readonly string _tableName;
        private readonly ConcurrentLru<long, string> _lru = new(1000);

        public ListTable(SqliteContext context, string tableName)
        {
            _context = context;
            _tableName = tableName;
        }

        public ValueTask CreateAsync()
        {
            return _context.Create(async (connection) =>
            {
                var command = connection.CreateCommand();
                command.CommandText = DatabaseCreateCommand;
                await command.ExecuteNonQueryAsync();
            });
        }

        public ValueTask DropAsync()
        {
            return _context.Write(async (connection) =>
            {
                var command = connection.CreateCommand();
                command.CommandText = DatabaseDropCommand;
                await command.ExecuteNonQueryAsync();
            });
        }

        public ValueTask<long> InsertAsync(string obj)
        {
            return _context.Write(async (connection) =>
            {
                var command = connection.CreateCommand();
                command.CommandText = $@"
                INSERT INTO {_tableName} (obj) VALUES (
                    $obj
                );
                SELECT last_insert_rowid();
                ";
                command.Parameters.AddWithValue("$obj", obj);
                var id = (long)(await command.ExecuteScalarAsync())!;

                _lru.GetOrAdd(id, _ => obj);
                return id;
            });
        }

        public ValueTask<string?> SearchAsync(long id)
        {
            if (_lru.TryGet(id, out var cachedObj))
            {
                return ValueTask.FromResult(cachedObj)!;
            }

            return _context.Read(async (connection) =>
            {
                var command = connection.CreateCommand();
                command.CommandText = $@"
                SELECT obj FROM {_tableName}
                    WHERE id=$id
                ";
                command.Parameters.AddWithValue("$id", id);
                var reader = await command.ExecuteReaderAsync();

                if (reader.Read())
                {
                    var obj = reader.GetString(0);

                    _lru.GetOrAdd(id, _ => obj);
                    return obj;
                }

                return null;
            });
        }
    }
}