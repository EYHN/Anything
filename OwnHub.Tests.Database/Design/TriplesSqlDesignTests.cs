using System;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using OwnHub.Database;
using OwnHub.Database.Provider;

namespace OwnHub.Tests.Database.Design
{
    public class TriplesSqlDesignTests
    {
        private static SqliteContext CreateSqliteContext(string name)
        {
            return new SqliteContext(new SharedMemoryConnectionProvider("TriplesSqlDesignTests-" + name));
        }

        [Test]
        public void SqlDesignTest()
        {
            var context = CreateSqliteContext("SqlDesignTest");
            var connection = context.GetCreateConnectionRef().Value;

            void RunSql(string sql)
            {
                var command = connection!.CreateCommand();
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }

            RunSql(@"
            PRAGMA recursive_triggers = ON;

            CREATE TABLE IF NOT EXISTS Triples (
                Id INTEGER PRIMARY KEY,
                Subject INTEGER NOT NULL,
                Predicate TEXT NOT NULL,
                Object,
                ObjectType TEXT NOT NULL
            );

            -- Unique index
            CREATE UNIQUE INDEX IF NOT EXISTS TriplesSubjectPredicateConstraint ON Triples (Subject, Predicate);
            CREATE UNIQUE INDEX IF NOT EXISTS TriplesObjectConstraint ON Triples (Object) WHERE ObjectType LIKE 'Object(_%)';

            -- ObjectType check, should match Value(_%) or Object(_%)
            CREATE TRIGGER IF NOT EXISTS TriplesCheckObjectTypeOnInsert INSERT ON Triples WHEN NEW.ObjectType NOT LIKE 'Value(_%)' AND NEW.ObjectType NOT LIKE 'Object(_%)'
                BEGIN
                SELECT RAISE(FAIL, 'ObjectType does not meet the constraints.');
                END;

            CREATE TRIGGER IF NOT EXISTS TriplesCheckObjectTypeOnUpdate UPDATE ON Triples WHEN NEW.ObjectType NOT LIKE 'Value(_%)' AND NEW.ObjectType NOT LIKE 'Object(_%)'
                BEGIN
                SELECT RAISE(FAIL, 'ObjectType does not meet the constraints.');
                END;

            -- Subject check, should existed in Object column.
            CREATE TRIGGER IF NOT EXISTS TriplesCheckSubjectOnInsert INSERT ON Triples WHEN NEW.Subject IS NOT 0 AND NOT EXISTS (SELECT Id FROM Triples WHERE Object=NEW.Subject AND ObjectType LIKE 'Object(_%)')
                BEGIN
                SELECT RAISE(FAIL, 'Subject object not found.');
                END;

            CREATE TRIGGER IF NOT EXISTS TriplesCheckSubjectOnUpdate UPDATE ON Triples WHEN NEW.Subject IS NOT 0 AND NOT EXISTS (SELECT Id FROM Triples WHERE Object=NEW.Subject AND ObjectType LIKE 'Object(_%)')
                BEGIN
                SELECT RAISE(FAIL, 'Subject object not found.');
                END;

            -- Delete check, the object should has no properties
            CREATE TRIGGER IF NOT EXISTS TriplesCheckOnDelete DELETE ON Triples WHEN OLD.ObjectType LIKE 'Object(_%)' AND EXISTS (SELECT Id FROM Triples WHERE Subject=OLD.Object)
                BEGIN
                SELECT RAISE(FAIL, 'Delete object should has no properties.');
                END;

            -- Update check, the object should has no properties
            CREATE TRIGGER IF NOT EXISTS TriplesCheckOnUpdate UPDATE ON Triples WHEN OLD.ObjectType LIKE 'Object(_%)' AND EXISTS (SELECT Id FROM Triples WHERE Subject=OLD.Object)
                BEGIN
                SELECT RAISE(FAIL, 'Update object should has no properties.');
                END;

            -- Performance optimization index.
            CREATE INDEX IF NOT EXISTS TriplesPredicateIndex ON Triples (Predicate);
            ");

            Assert.Throws<SqliteException>(() => RunSql(@"INSERT INTO Triples (Subject, Predicate, Object, ObjectType) VALUES(0, 'prop', 'a', 'String')"));

            RunSql(@"INSERT INTO Triples (Subject, Predicate, Object, ObjectType) VALUES(0, 'prop', 'a', 'Value(String)')");

            Assert.Throws<SqliteException>(() => RunSql(@"UPDATE Triples SET ObjectType='Object()' WHERE Subject=0 AND Predicate='prop'"));
            Assert.Throws<SqliteException>(() => RunSql(@"INSERT OR REPLACE INTO Triples (Subject, Predicate, Object, ObjectType) VALUES(0, 'prop', 'a', 'String')"));

            RunSql(@"UPDATE Triples SET ObjectType='Object(File)' WHERE Subject=0 AND Predicate='prop'");
            RunSql(@"
            INSERT INTO Triples (Subject, Predicate, Object, ObjectType) VALUES(0, 'prop1', 1, 'Object(File)');
            INSERT INTO Triples (Subject, Predicate, Object, ObjectType) VALUES(1, 'child', 'hello', 'Value(String)');
            ");

            Assert.Throws<SqliteException>(() => RunSql(@"INSERT INTO Triples (Subject, Predicate, Object, ObjectType) VALUES(2, 'child2', 'hello', 'Value(String)')"));
            Assert.Throws<SqliteException>(() => RunSql(@"UPDATE Triples SET Subject=2 WHERE Predicate='child' AND Object='hello' AND ObjectType='Value(String)'"));

            RunSql(@"
            UPDATE Triples SET Subject=1 WHERE Predicate='child' AND Object='hello' AND ObjectType='Value(String)';
            UPDATE Triples SET Subject=0 WHERE Predicate='child' AND Object='hello' AND ObjectType='Value(String)';
            ");

            RunSql(@"INSERT INTO Triples (Subject, Predicate, Object, ObjectType) VALUES(1, 'prop1', 2, 'Object(File)')");
            Console.WriteLine(Assert.Throws<SqliteException>(() => RunSql(@"DELETE FROM Triples WHERE Subject=0 AND Predicate='prop1'")));
            Console.WriteLine(Assert.Throws<SqliteException>(() => RunSql(@"UPDATE Triples SET Object='a', ObjectType='Value(String)' WHERE Subject=0 AND Predicate='prop1'")));
            Console.WriteLine(Assert.Throws<SqliteException>(() => RunSql(@"INSERT OR REPLACE INTO Triples (Subject, Predicate, Object, ObjectType) VALUES(0, 'prop1', 'a', 'Value(String)')")));
            RunSql(@"DELETE FROM Triples WHERE Subject=1 AND Predicate='prop1';");
            RunSql(@"DELETE FROM Triples WHERE Subject=0 AND Predicate='prop1';");
        }
    }
}
