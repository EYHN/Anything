using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace OwnHub.Sqlite.Table
{
    /// <summary>
    /// Use sqlite rtree extension to index 3D vectors and support range query.
    /// </summary>
    public class Vector3IndexTable : Table
    {
        protected override string DatabaseDropCommand => $@"
            DROP TABLE IF EXISTS {TableName};
            ";

        protected override string DatabaseCreateCommand => $@"
            CREATE VIRTUAL TABLE IF NOT EXISTS {TableName} USING rtree(
                id,
                minX, maxX,
                minY, maxY,
                minZ, maxZ,
                +extraData TEXT
            );
            ";

        public Vector3IndexTable(string tableName)
            : base(tableName)
        {
        }

        public async ValueTask InsertAsync(SqliteConnection connection, long id, Vector3 v, string? extraData = null)
        {
            var command = connection.CreateCommand();
            command.CommandText = extraData != null
                ? $@"INSERT INTO {TableName} (id, minX, maxX, minY, maxY, minZ, maxZ, extraData) VALUES(
                    $id, $minX, $maxX, $minY, $maxY, $minZ, $maxZ, $extraData
                );"
                : $@"INSERT INTO {TableName} (id, minX, maxX, minY, maxY, minZ, maxZ) VALUES(
                    $id, $minX, $maxX, $minY, $maxY, $minZ, $maxZ
                );";
            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$minX", v.X);
            command.Parameters.AddWithValue("$maxX", v.X);
            command.Parameters.AddWithValue("$minY", v.Y);
            command.Parameters.AddWithValue("$maxY", v.Y);
            command.Parameters.AddWithValue("$minZ", v.Z);
            command.Parameters.AddWithValue("$maxZ", v.Z);
            if (extraData != null)
            {
                command.Parameters.AddWithValue("$extraData", extraData);
            }

            await command.ExecuteNonQueryAsync();
        }

        public void Insert(SqliteConnection connection, long id, Vector3 v, string? extraData = null)
        {
            var command = connection.CreateCommand();
            command.CommandText = extraData != null
                ? $@"INSERT INTO {TableName} (id, minX, maxX, minY, maxY, minZ, maxZ, extraData) VALUES(
                    $id, $minX, $maxX, $minY, $maxY, $minZ, $maxZ, $extraData
                );"
                : $@"INSERT INTO {TableName} (id, minX, maxX, minY, maxY, minZ, maxZ) VALUES(
                    $id, $minX, $maxX, $minY, $maxY, $minZ, $maxZ
                );";
            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$minX", v.X);
            command.Parameters.AddWithValue("$maxX", v.X);
            command.Parameters.AddWithValue("$minY", v.Y);
            command.Parameters.AddWithValue("$maxY", v.Y);
            command.Parameters.AddWithValue("$minZ", v.Z);
            command.Parameters.AddWithValue("$maxZ", v.Z);
            if (extraData != null)
            {
                command.Parameters.AddWithValue("$extraData", extraData);
            }

            command.ExecuteNonQuery();
        }

        public void Update(SqliteConnection connection, long id, Vector3 v, string? extraData = null)
        {
            var command = connection.CreateCommand();
            command.CommandText = extraData != null
            ? $@"UPDATE {TableName}
            SET minX=$minX, minY=$minY, minZ=$minZ, maxX=$maxX, maxY=$maxY, maxZ=$maxZ, extraData=$extraData
            WHERE id=$id;"
            : $@"UPDATE {TableName}
            SET minX=$minX, minY=$minY, minZ=$minZ, maxX=$maxX, maxY=$maxY, maxZ=$maxZ
            WHERE id=$id;";
            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$minX", v.X);
            command.Parameters.AddWithValue("$maxX", v.X);
            command.Parameters.AddWithValue("$minY", v.Y);
            command.Parameters.AddWithValue("$maxY", v.Y);
            command.Parameters.AddWithValue("$minZ", v.Z);
            command.Parameters.AddWithValue("$maxZ", v.Z);
            if (extraData != null)
            {
                command.Parameters.AddWithValue("$extraData", extraData);
            }

            command.ExecuteNonQuery();
        }

        public async ValueTask UpdateAsync(SqliteConnection connection, long id, Vector3 v, string? extraData = null)
        {
            var command = connection.CreateCommand();
            command.CommandText = extraData != null
            ? $@"UPDATE {TableName}
            SET minX=$minX, minY=$minY, minZ=$minZ, maxX=$maxX, maxY=$maxY, maxZ=$maxZ, extraData=$extraData
            WHERE id=$id;"
            : $@"UPDATE {TableName}
            SET minX=$minX, minY=$minY, minZ=$minZ, maxX=$maxX, maxY=$maxY, maxZ=$maxZ
            WHERE id=$id;";
            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$minX", v.X);
            command.Parameters.AddWithValue("$maxX", v.X);
            command.Parameters.AddWithValue("$minY", v.Y);
            command.Parameters.AddWithValue("$maxY", v.Y);
            command.Parameters.AddWithValue("$minZ", v.Z);
            command.Parameters.AddWithValue("$maxZ", v.Z);
            if (extraData != null)
            {
                command.Parameters.AddWithValue("$extraData", extraData);
            }

            await command.ExecuteNonQueryAsync();
        }

        public Row? Select(SqliteConnection connection, long id)
        {
            var command = connection.CreateCommand();
            command.CommandText = $@"
            SELECT minX, minY, minZ, extraData FROM {TableName}
                WHERE id=$id;
            ";
            command.Parameters.AddWithValue("$id", id);
            var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            var x = reader.GetFloat(0);
            var y = reader.GetFloat(1);
            var z = reader.GetFloat(2);
            var extraData = !reader.IsDBNull(3) ? reader.GetString(3) : null;
            var result = new Row(id, new Vector3(x, y, z), extraData);
#if DEBUG
            Debug.Assert(reader.Read() == false, "The reader should be ended.");
#endif
            return result;
        }

        public async ValueTask<Row?> SelectAsync(SqliteConnection connection, long id)
        {
            var command = connection.CreateCommand();
            command.CommandText = $@"
            SELECT minX, minY, minZ, extraData FROM {TableName}
                WHERE id=$id;
            ";
            command.Parameters.AddWithValue("$id", id);
            var reader = await command.ExecuteReaderAsync();

            if (!reader.Read())
            {
                return null;
            }

            var x = reader.GetFloat(0);
            var y = reader.GetFloat(1);
            var z = reader.GetFloat(2);
            var extraData = !reader.IsDBNull(3) ? reader.GetString(3) : null;
            var result = new Row(id, new Vector3(x, y, z), extraData);
#if DEBUG
            Debug.Assert(reader.Read() == false, "The reader should be ended.");
#endif
            return result;
        }

        public IEnumerable<Row> Search(
            SqliteConnection connection,
            Vector3 minV,
            Vector3 maxV)
        {
            var command = connection.CreateCommand();
            command.CommandText = $@"
            SELECT id, minX, minY, minZ, extraData FROM {TableName}
                WHERE minX>=$minX AND maxX<=$maxX
                  AND minY>=$minY AND maxY<=$maxY
                  AND minZ>=$minZ AND maxZ<=$maxZ
            ";
            command.Parameters.AddWithValue("$minX", minV.X);
            command.Parameters.AddWithValue("$maxX", maxV.X);
            command.Parameters.AddWithValue("$minY", minV.Y);
            command.Parameters.AddWithValue("$maxY", maxV.Y);
            command.Parameters.AddWithValue("$minZ", minV.Z);
            command.Parameters.AddWithValue("$maxZ", maxV.Z);
            var reader = command.ExecuteReader();

            var result = new List<Row>();

            while (reader.Read())
            {
                var id = reader.GetInt64(0);
                var x = reader.GetFloat(1);
                var y = reader.GetFloat(2);
                var z = reader.GetFloat(3);
                var extraData = !reader.IsDBNull(4) ? reader.GetString(4) : null;
                result.Add(new Row(id, new Vector3(x, y, z), extraData));
            }

            return result;
        }

        public async ValueTask<IEnumerable<Row>> SearchAsync(
            SqliteConnection connection,
            Vector3 minV,
            Vector3 maxV)
        {
            var command = connection.CreateCommand();
            command.CommandText = $@"
            SELECT id, minX, minY, minZ, extraData FROM {TableName}
                WHERE minX>=$minX AND maxX<=$maxX
                  AND minY>=$minY AND maxY<=$maxY
                  AND minZ>=$minZ AND maxZ<=$maxZ
            ";
            command.Parameters.AddWithValue("$minX", minV.X);
            command.Parameters.AddWithValue("$maxX", maxV.X);
            command.Parameters.AddWithValue("$minY", minV.Y);
            command.Parameters.AddWithValue("$maxY", maxV.Y);
            command.Parameters.AddWithValue("$minZ", minV.Z);
            command.Parameters.AddWithValue("$maxZ", maxV.Z);
            var reader = await command.ExecuteReaderAsync();

            var result = new List<Row>();

            while (reader.Read())
            {
                var id = reader.GetInt64(0);
                var x = reader.GetFloat(1);
                var y = reader.GetFloat(2);
                var z = reader.GetFloat(3);
                var extraData = !reader.IsDBNull(4) ? reader.GetString(4) : null;
                result.Add(new Row(id, new Vector3(x, y, z), extraData));
            }

            return result;
        }

        public void Delete(SqliteConnection connection, long id)
        {
            var command = connection.CreateCommand();
            command.CommandText = $@"DELETE FROM {TableName} WHERE id=$id";
            command.Parameters.AddWithValue("$id", id);
            command.ExecuteNonQuery();
        }

        public async ValueTask DeleteAsync(SqliteConnection connection, long id)
        {
            var command = connection.CreateCommand();
            command.CommandText = $@"DELETE FROM {TableName} WHERE id=$id";
            command.Parameters.AddWithValue("$id", id);
            await command.ExecuteNonQueryAsync();
        }

        public record Row(long Id, Vector3 V, string? ExtraData);
    }
}
