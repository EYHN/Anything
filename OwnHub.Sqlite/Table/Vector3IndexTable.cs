using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using OwnHub.Utils.Color;

namespace OwnHub.Sqlite.Table
{
    /// <summary>
    /// Use sqlite rtree extension to index 3D vectors and support range query
    /// </summary>
    public class Vector3IndexTable
    {
        private string DatabaseDropCommand => $@"
            DROP TABLE IF EXISTS {tableName};
            ";

        private string DatabaseCreateCommand => $@"
            CREATE VIRTUAL TABLE IF NOT EXISTS {tableName} USING rtree(
                id,
                minX, maxX,
                minY, maxY,
                minZ, maxZ,
                +extraData TEXT
            );
            ";
        
        private readonly SqliteContext context;
        private readonly string tableName;
        
        public Vector3IndexTable(SqliteContext context, string tableName)
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
        
        public Task Insert(string id, Vector3 v)
        {
            return context.Write(async (connection) =>
            {
                SqliteCommand command = connection.CreateCommand();
                command.CommandText = $@"
                INSERT INTO {tableName} (id, minX, maxX, minY, maxY, minZ, maxZ) VALUES(
                    $id, $minX, $maxX, $minY, $maxY, $minZ, $maxZ
                );
                ";
                command.Parameters.AddWithValue("$id", id);
                command.Parameters.AddWithValue("$minX", v.X);
                command.Parameters.AddWithValue("$maxX", v.X);
                command.Parameters.AddWithValue("$minY", v.Y);
                command.Parameters.AddWithValue("$maxY", v.Y);
                command.Parameters.AddWithValue("$minZ", v.Z);
                command.Parameters.AddWithValue("$maxZ", v.Z);
                await command.ExecuteNonQueryAsync();
            });
        }
        
        public Task Insert(string id, Vector3 v, string extraData)
        {
            return context.Write(async (connection) =>
            {
                SqliteCommand command = connection.CreateCommand();
                command.CommandText = $@"
                INSERT INTO {tableName} (id, minX, maxX, minY, maxY, minZ, maxZ, extraData) VALUES(
                    $id, $minX, $maxX, $minY, $maxY, $minZ, $maxZ, $extraData
                );
                ";
                command.Parameters.AddWithValue("$id", id);
                command.Parameters.AddWithValue("$minX", v.X);
                command.Parameters.AddWithValue("$maxX", v.X);
                command.Parameters.AddWithValue("$minY", v.Y);
                command.Parameters.AddWithValue("$maxY", v.Y);
                command.Parameters.AddWithValue("$minZ", v.Z);
                command.Parameters.AddWithValue("$maxZ", v.Z);
                command.Parameters.AddWithValue("$extraData", extraData);
                await command.ExecuteNonQueryAsync();
            });
        }
        
        public Task<IEnumerable<Row>> Search(
            Vector3 minV,
            Vector3 maxV
            )
        {
            return context.Read(async (connection) =>
            {
                SqliteCommand command = connection.CreateCommand();
                command.CommandText = $@"
                SELECT id, minX, minY, minZ, extraData FROM {tableName}
                    WHERE minX>=$minX AND maxX<=$maxX
                      AND minY>=$minY AND maxY<=$maxY
                      AND minZ>=$minZ AND maxZ<=$maxZ
                ";;
                command.Parameters.AddWithValue("$minX", minV.X);
                command.Parameters.AddWithValue("$maxX", maxV.X);
                command.Parameters.AddWithValue("$minY", minV.Y);
                command.Parameters.AddWithValue("$maxY", maxV.Y);
                command.Parameters.AddWithValue("$minZ", minV.Z);
                command.Parameters.AddWithValue("$maxZ", maxV.Z);
                SqliteDataReader reader = await command.ExecuteReaderAsync();
                
                var result = new List<Row>();
                
                while (reader.Read())
                {
                    string id = reader.GetString(0);
                    float x = reader.GetFloat(1);
                    float y = reader.GetFloat(2);
                    float z = reader.GetFloat(3);
                    string? extraData = !reader.IsDBNull(4) ? reader.GetString(4) : null;
                    result.Add(new Row()
                    {
                        Id = id,
                        V = new Vector3(x,y,z),
                        ExtraData = extraData
                    });
                }

                return (IEnumerable<Row>) result;
            });
        }

        public Task Delete(string id)
        {
            return context.Read(async (connection) =>
            {
                SqliteCommand command = connection.CreateCommand();
                command.CommandText = $@"DELETE FROM {tableName} WHERE id=$id";;
                command.Parameters.AddWithValue("$id", id);
                await command.ExecuteNonQueryAsync();
            });
        }
        
        public class Row
        {
            public string Id = null!;
            public Vector3 V;
            public string? ExtraData;

            internal Row()
            {
                
            }
        }
    }
}