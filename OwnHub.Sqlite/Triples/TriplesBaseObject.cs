using System;
using System.Collections.Generic;
using OwnHub.Sqlite.Orm;

namespace OwnHub.Sqlite.Triples
{
    public class TriplesBaseObject : IOrmLifeCycle
    {
        public enum StateMode
        {
            Pending = 0,
            Managed = 1,
            Released = 2
        }

        public OrmSystem OrmSystem { get; set; } = null!;

        protected OrmInstanceInfo InstanceInfo { get; private set; } = null!;

        protected OrmTransaction Transaction => InstanceInfo.Transaction;

        public StateMode State { get; private set; } = StateMode.Pending;

        private readonly Dictionary<string, object?> _pendingChanging = new();

        public object? this[string key]
        {
            get => GetProperty(key);

            set => SetProperty(key, value);
        }

        private object? GetProperty(string name)
        {
            object? value;
            if (State == StateMode.Pending)
            {
                // Conforms to the triples database logic, null returns false.
                _pendingChanging.TryGetValue(name, out value);
                return value;
            }

            if (State == StateMode.Managed)
            {
                if (_pendingChanging.TryGetValue(name, out value))
                {
                    // has pending changing
                    return value;
                }
                else
                {
                    // read from database
                    return GetPropertyDatabase(name);
                }
            }

            return null;
        }

        private void SetProperty(string name, object? value)
        {
            _pendingChanging[name] = value;
        }

        private object? GetPropertyDatabase(string name)
        {
            if (State != StateMode.Managed)
            {
                throw new InvalidOperationException("Invalid state.");
            }

            if (OrmSystem.DatabaseProvider is TriplesDatabaseProvider triplesDatabaseProvider)
            {
                return triplesDatabaseProvider.ReadProperty(Transaction, InstanceInfo.ObjectId, name, out _);
            }
            else
            {
                throw new InvalidOperationException($"{nameof(TriplesBaseObject)} only work with {nameof(TriplesDatabaseProvider)}");
            }
        }

        private void CommitOfflineCache()
        {
            if (State != StateMode.Managed)
            {
                throw new InvalidOperationException("Invalid state.");
            }

            if (OrmSystem.DatabaseProvider is TriplesDatabaseProvider triplesDatabaseProvider)
            {
                foreach (var pair in _pendingChanging)
                {
                    triplesDatabaseProvider.InsertOrReplaceProperty(Transaction, InstanceInfo.ObjectId, pair.Key, pair.Value);
                }
            }
            else
            {
                throw new InvalidOperationException($"{nameof(TriplesBaseObject)} only work with {nameof(TriplesDatabaseProvider)}");
            }

            _pendingChanging.Clear();
        }

        public void OnWillCreate(IOrmLifeCycle.OrmLifeCycleWillCreateEvent willCreateEvent)
        {
        }

        public void OnDidCreate(IOrmLifeCycle.OrmLifeCycleDidCreateEvent didCreateEvent)
        {
            OrmSystem = didCreateEvent.OrmSystem;
            InstanceInfo = didCreateEvent.CurrentInstance;
            State = StateMode.Managed;
            CommitOfflineCache();
        }

        public void OnDidRestore(IOrmLifeCycle.OrmLifeCycleDidRestoreEvent didRestoreEvent)
        {
            OrmSystem = didRestoreEvent.OrmSystem;
            InstanceInfo = didRestoreEvent.CurrentInstance;
            State = StateMode.Managed;
            CommitOfflineCache();
        }

        public void OnWillUpdate(IOrmLifeCycle.OrmLifeCycleWillUpdateEvent willUpdateEvent)
        {
        }

        public void OnDidUpdate(IOrmLifeCycle.OrmLifeCycleDidUpdateEvent didUpdateEvent)
        {
            CommitOfflineCache();
        }
    }
}
