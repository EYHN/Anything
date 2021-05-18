using System;
using System.Collections.Generic;

namespace StagingBox.Database.Orm
{
    public class OrmInstanceInfo
    {
        /// <summary>
        /// Gets or sets the saved state of the instance.
        /// </summary>
        internal OrmSnapshot? SavedState { get; set; } = null;

        /// <summary>
        /// Gets the object id of the instance.
        /// </summary>
        public long ObjectId { get; }

        /// <summary>
        /// Gets target instance.
        /// </summary>
        public object TargetInstance { get; }

        /// <summary>
        /// Gets the type info of the instance.
        /// </summary>
        public OrmTypeInfo TypeInfo { get; }

        /// <summary>
        /// Gets the transaction associated with the instance.
        /// </summary>
        public OrmTransaction Transaction { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrmInstanceInfo"/> class.
        /// </summary>
        /// <param name="transaction">the transaction associated of the instance.</param>
        /// <param name="objectId">object id of the instance.</param>
        /// <param name="targetInstance">target dotnet instance.</param>
        /// <param name="typeInfo">type info of the instance.</param>
        public OrmInstanceInfo(OrmTransaction transaction, long objectId, object targetInstance, OrmTypeInfo typeInfo)
        {
            Transaction = transaction;
            ObjectId = objectId;
            TargetInstance = targetInstance;
            TypeInfo = typeInfo;
        }

        /// <summary>
        /// Take a snapshot of the instance.
        /// </summary>
        /// <returns>Snapshot for the instance.</returns>
        public OrmSnapshot TakeSnapshot()
        {
            return TakeSnapshot(TargetInstance, TypeInfo);
        }

        /// <summary>
        /// Take a snapshot of the given instance.
        /// </summary>
        /// <param name="instance">The instance to take snapshot.</param>
        /// <param name="typeInfo">The type info of the instance.</param>
        /// <returns>Snapshot for the instance.</returns>
        public static OrmSnapshot TakeSnapshot(object instance, OrmTypeInfo typeInfo)
        {
            OrmSnapshot data = new();
            foreach (var property in typeInfo.Properties)
            {
                var value = property.ReadValue(instance);
                data.Add(property, value);
            }

            return data;
        }

        /// <summary>
        /// Compare the saved snapshot with the given snapshot.
        /// </summary>
        /// <param name="secondSnapshot">the snapshot to diff.</param>
        /// <returns>the result of diff.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public IEnumerable<OrmSnapshot.DiffResult> DiffChange(OrmSnapshot secondSnapshot)
        {
            if (SavedState == null)
            {
                throw new InvalidOperationException("No snapshot is saved. ");
            }

            return OrmSnapshot.Diff(SavedState, secondSnapshot);
        }
    }
}
