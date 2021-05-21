using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace Anything.Database.Table
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

        private string InsertWithExtraDataCommand => $@"
            INSERT INTO {TableName} (id, minX, maxX, minY, maxY, minZ, maxZ, extraData) VALUES(?1, ?2, ?3, ?4, ?5, ?6, ?7, ?8);";

        private string InsertCommand => $@"
            INSERT INTO {TableName} (id, minX, maxX, minY, maxY, minZ, maxZ) VALUES(?1, ?2, ?3, ?4, ?5, ?6, ?7);";

        private string UpdateWithExtraDataCommand => $@"UPDATE {TableName}
            SET minX=?1, minY=?2, minZ=?3, maxX=?4, maxY=?5, maxZ=?6, extraData=?7
            WHERE id=?8;";

        private string UpdateCommand => $@"UPDATE {TableName}
            SET minX=?1, minY=?2, minZ=?3, maxX=?4, maxY=?5, maxZ=?6
            WHERE id=?7;";

        private string SelectCommand => $@"
            SELECT minX, minY, minZ, extraData FROM {TableName}
                WHERE id=?1;";

        private string SearchCommand => $@"
            SELECT id, minX, minY, minZ, extraData FROM {TableName}
                WHERE minX>=?1 AND maxX<=?2
                  AND minY>=?3 AND maxY<=?4
                  AND minZ>=?5 AND maxZ<=?6;";

        private string DeleteCommand => $@"DELETE FROM {TableName} WHERE id=?1";

        public Vector3IndexTable(string tableName)
            : base(tableName)
        {
        }

        public async ValueTask InsertAsync(IDbTransaction transaction, long id, Vector3 v, string? extraData = null)
        {
            if (extraData != null)
            {
                await transaction.ExecuteNonQueryAsync(
                    () => InsertWithExtraDataCommand,
                    $"{nameof(Vector3IndexTable)}/{nameof(InsertWithExtraDataCommand)}/{TableName}",
                    id,
                    v.X,
                    v.X,
                    v.Y,
                    v.Y,
                    v.Z,
                    v.Z,
                    extraData);
            }
            else
            {
                await transaction.ExecuteNonQueryAsync(
                    () => InsertCommand,
                    $"{nameof(Vector3IndexTable)}/{nameof(InsertCommand)}/{TableName}",
                    id,
                    v.X,
                    v.X,
                    v.Y,
                    v.Y,
                    v.Z,
                    v.Z);
            }
        }

        public void Insert(IDbTransaction transaction, long id, Vector3 v, string? extraData = null)
        {
            if (extraData != null)
            {
                transaction.ExecuteNonQuery(
                    () => InsertWithExtraDataCommand,
                    $"{nameof(Vector3IndexTable)}/{nameof(InsertWithExtraDataCommand)}/{TableName}",
                    id,
                    v.X,
                    v.X,
                    v.Y,
                    v.Y,
                    v.Z,
                    v.Z,
                    extraData);
            }
            else
            {
                transaction.ExecuteNonQuery(
                    () => InsertCommand,
                    $"{nameof(Vector3IndexTable)}/{nameof(InsertCommand)}/{TableName}",
                    id,
                    v.X,
                    v.X,
                    v.Y,
                    v.Y,
                    v.Z,
                    v.Z);
            }
        }

        public void Update(IDbTransaction transaction, long id, Vector3 v, string? extraData = null)
        {
            if (extraData != null)
            {
                transaction.ExecuteNonQuery(
                    () => UpdateWithExtraDataCommand,
                    $"{nameof(Vector3IndexTable)}/{nameof(UpdateWithExtraDataCommand)}/{TableName}",
                    v.X,
                    v.X,
                    v.Y,
                    v.Y,
                    v.Z,
                    v.Z,
                    extraData,
                    id);
            }
            else
            {
                transaction.ExecuteNonQuery(
                    () => UpdateCommand,
                    $"{nameof(Vector3IndexTable)}/{nameof(UpdateCommand)}/{TableName}",
                    v.X,
                    v.X,
                    v.Y,
                    v.Y,
                    v.Z,
                    v.Z,
                    id);
            }
        }

        public async ValueTask UpdateAsync(IDbTransaction transaction, long id, Vector3 v, string? extraData = null)
        {
            if (extraData != null)
            {
                await transaction.ExecuteNonQueryAsync(
                    () => UpdateWithExtraDataCommand,
                    $"{nameof(Vector3IndexTable)}/{nameof(UpdateWithExtraDataCommand)}/{TableName}",
                    v.X,
                    v.X,
                    v.Y,
                    v.Y,
                    v.Z,
                    v.Z,
                    extraData,
                    id);
            }
            else
            {
                await transaction.ExecuteNonQueryAsync(
                    () => UpdateCommand,
                    $"{nameof(Vector3IndexTable)}/{nameof(UpdateCommand)}/{TableName}",
                    v.X,
                    v.X,
                    v.Y,
                    v.Y,
                    v.Z,
                    v.Z,
                    id);
            }
        }

        public Row? Select(IDbTransaction transaction, long id)
        {
            return transaction.ExecuteReader(
                () => SelectCommand,
                $"{nameof(Vector3IndexTable)}/{nameof(SelectCommand)}/{TableName}",
                (reader) =>
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    var x = reader.GetFloat(0);
                    var y = reader.GetFloat(1);
                    var z = reader.GetFloat(2);
                    var extraData = !reader.IsDBNull(3) ? reader.GetString(3) : null;
                    return new Row(id, new Vector3(x, y, z), extraData);
                },
                id);
        }

        public async ValueTask<Row?> SelectAsync(IDbTransaction transaction, long id)
        {
            return await transaction.ExecuteReaderAsync(
                () => SelectCommand,
                $"{nameof(Vector3IndexTable)}/{nameof(SelectCommand)}/{TableName}",
                (reader) =>
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    var x = reader.GetFloat(0);
                    var y = reader.GetFloat(1);
                    var z = reader.GetFloat(2);
                    var extraData = !reader.IsDBNull(3) ? reader.GetString(3) : null;
                    return new Row(id, new Vector3(x, y, z), extraData);
                },
                id);
        }

        public IEnumerable<Row> Search(
            IDbTransaction transaction,
            Vector3 minV,
            Vector3 maxV)
        {
            return transaction.ExecuteReader(
                () => SearchCommand,
                $"{nameof(Vector3IndexTable)}/{nameof(SearchCommand)}/{TableName}",
                (reader) =>
                {
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
                },
                minV.X,
                maxV.X,
                minV.Y,
                maxV.Y,
                minV.Z,
                maxV.Z);
        }

        public async ValueTask<IEnumerable<Row>> SearchAsync(
            IDbTransaction transaction,
            Vector3 minV,
            Vector3 maxV)
        {
            return await transaction.ExecuteReaderAsync(
                () => SearchCommand,
                $"{nameof(Vector3IndexTable)}/{nameof(SearchCommand)}/{TableName}",
                (reader) =>
                {
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
                },
                minV.X,
                maxV.X,
                minV.Y,
                maxV.Y,
                minV.Z,
                maxV.Z);
        }

        public void Delete(IDbTransaction transaction, long id)
        {
            transaction.ExecuteNonQuery(
                () => DeleteCommand,
                $"{nameof(Vector3IndexTable)}/{nameof(DeleteCommand)}/{TableName}",
                id);
        }

        public async ValueTask DeleteAsync(IDbTransaction transaction, long id)
        {
            await transaction.ExecuteNonQueryAsync(
                () => DeleteCommand,
                $"{nameof(Vector3IndexTable)}/{nameof(DeleteCommand)}/{TableName}",
                id);
        }

        public record Row(long Id, Vector3 V, string? ExtraData);
    }
}
