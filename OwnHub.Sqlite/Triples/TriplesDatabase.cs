using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using OwnHub.Sqlite.Table;

namespace OwnHub.Sqlite.Triples
{
    public sealed class TriplesDatabase
    {
        public TriplesRoot Root { get; }

        internal TriplesObjectTypeRegistry TypeRegistry { get; }

        internal TriplesSerializer Serializer { get; }

        internal TriplesTable TriplesTable { get; }

        internal TriplesSequenceTable TriplesSequenceTable { get; }

        internal SqliteContext SqliteContext { get; }

        internal const string SequenceName = "Object";

        private readonly Dictionary<Type, Table.Table> _externalTableList = new();

        private bool _isCreated = false;

        public string TableName { get; }

        public TriplesDatabase(SqliteContext context, string tableName)
        {
            SqliteContext = context;
            TableName = tableName;
            TriplesTable = new TriplesTable(tableName);
            TypeRegistry = new TriplesObjectTypeRegistry();
            Serializer = new TriplesSerializer(TypeRegistry);
            TriplesSequenceTable = new TriplesSequenceTable(tableName + "Sequence");
            RegisterObjectType<TriplesRoot>();
            Root = new TriplesRoot(this);
        }

        public async ValueTask CreateAsync()
        {
            using var connectionRef = SqliteContext.GetCreateConnectionRef();
            var connection = connectionRef.Value;
            await TriplesTable.CreateAsync(connection);
            await TriplesSequenceTable.CreateAsync(connection);
            await TriplesSequenceTable.InsertAsync(connection, SequenceName, ignoreIfExist: true);

            foreach (var externalTable in _externalTableList.Values)
            {
                await externalTable.CreateAsync(connection);
            }

            _isCreated = true;
        }

        public void Create()
        {
            using var connectionRef = SqliteContext.GetCreateConnectionRef();
            var connection = connectionRef.Value;
            TriplesTable.Create(connection);
            TriplesSequenceTable.Create(connection);
            TriplesSequenceTable.Insert(connection, SequenceName, ignoreIfExist: true);

            foreach (var externalTable in _externalTableList.Values)
            {
                externalTable.Create(connection);
            }

            _isCreated = true;
        }

        public async ValueTask DropAsync()
        {
            using var connectionRef = SqliteContext.GetWriteConnectionRef();
            var connection = connectionRef.Value;
            await TriplesTable.DropAsync(connection);
            await TriplesSequenceTable.DropAsync(connection);

            foreach (var externalTable in _externalTableList.Values)
            {
                await externalTable.DropAsync(connection);
            }

            _isCreated = false;
        }

        public void Drop()
        {
            using var connectionRef = SqliteContext.GetWriteConnectionRef();
            var connection = connectionRef.Value;
            TriplesTable.Drop(connection);
            TriplesSequenceTable.Drop(connection);

            foreach (var externalTable in _externalTableList.Values)
            {
                externalTable.Drop(connection);
            }

            _isCreated = false;
        }

        public T GetExternalTable<T>()
                where T : Table.Table
        {
            return (T)GetExternalTable(typeof(T));
        }

        public Table.Table GetExternalTable(Type type)
        {
            return _externalTableList[type];
        }

        public void RegisterObjectType<T>()
            where T : TriplesObject
        {
            RegisterObjectType(typeof(T));
        }

        public void RegisterObjectType(Type type)
        {
            if (_isCreated)
            {
                throw new InvalidOperationException("Can't register type after the database has been created.");
            }

            if (type == typeof(TriplesObject) || !type.IsAssignableTo(typeof(TriplesObject)))
            {
                throw new ArgumentException("Invalid type: " + type.Name + ". The type must be a subclass of TriplesObject.");
            }

            TriplesTypeDesc typeDesc;

            var typeName = type.GetCustomAttribute<TriplesTypeNameAttribute>()?.TypeName ?? type.Name;

            var externalTableAttribute = type.GetCustomAttribute<TriplesExternalTableAttribute>();
            if (externalTableAttribute != null)
            {
                var externalTableType = externalTableAttribute.TableType;
                var externalTableName = externalTableAttribute.TableName;

                var instance = Activator.CreateInstance(externalTableType, TableName + "_" + externalTableName);
                if (instance is Table.Table table)
                {
                    _externalTableList.Add(externalTableType, table);
                }
                else
                {
                    throw new InvalidOperationException("Can't create instance of type: " + externalTableType.Name);
                }
            }

            typeDesc = new TriplesTypeDesc(TriplesTypeCategory.Object, typeName);

            TypeRegistry.Registry(type, typeDesc);
        }
    }
}
