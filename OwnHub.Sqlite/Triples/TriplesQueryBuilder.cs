using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Data.Sqlite;

namespace OwnHub.Sqlite.Triples
{
    public class TriplesQueryBuilder
    {
        private TriplesDatabase _database;
        private TriplesTransaction _transaction;
        private SqliteCommand _command;
        private StringBuilder _whereClause = new();

        public SqliteCommand Command
        {
            get
            {
                _command.CommandText = @$"SELECT Subject FROM {_database.TableName} WHERE " + _whereClause.ToString();
                return _command;
            }
        }

        public TriplesQueryBuilder(TriplesTransaction transaction)
        {
            _transaction = transaction;
            _database = transaction.Database;
            _command = transaction.DbConnection.CreateCommand();
        }

        public TriplesQueryBuilder WhereEquals(string predicate, object obj)
        {
            _database.Serializer.Serialize(obj, out var data, out var typeDesc);
            _whereClause.AppendFormat(
                $@" Predicate={AddParameter(predicate)} AND Object={AddParameter(data)} AND ObjectType={AddParameter(typeDesc.ToTypeDescText())} ");
            return this;
        }

        public TriplesQueryBuilder And()
        {
            _whereClause.AppendFormat(" AND ");
            return this;
        }

        private string AddParameter(object obj)
        {
            var index = _command.Parameters.Add(obj);
            return "{" + index + "}";
        }
    }
}
