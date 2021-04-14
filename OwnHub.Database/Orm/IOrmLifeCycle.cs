using System;

namespace OwnHub.Database.Orm
{
    /// <summary>
    /// The interface contains handlers for the orm life cycle.
    /// </summary>
    public interface IOrmLifeCycle
    {
        /// <summary>
        /// The method which will called before saving to the database. You are able to set the value of properties at this point.
        /// </summary>
        /// <param name="willCreateEvent">the event data.</param>
        public void OnWillCreate(OrmLifeCycleWillCreateEvent willCreateEvent);

        /// <summary>
        /// The method which will called after saving to the database. You can read and write to the database at this point.
        /// </summary>
        /// <param name="didCreateEvent">the event data.</param>
        public void OnDidCreate(OrmLifeCycleDidCreateEvent didCreateEvent);

        /// <summary>
        /// The method which will called before updating the database. You are able to set the value of properties at this point.
        /// </summary>
        /// <param name="willUpdateEvent">the event data.</param>
        public void OnWillUpdate(OrmLifeCycleWillUpdateEvent willUpdateEvent);

        /// <summary>
        /// The method which will called after updating the database. You can read and write to the database at this point.
        /// </summary>
        /// <param name="didUpdateEvent">the event data.</param>
        public void OnDidUpdate(OrmLifeCycleDidUpdateEvent didUpdateEvent);

        /// <summary>
        /// The method which will called after restore the database. You can read and write to the database at this point.
        /// </summary>
        /// <param name="didRestoreEvent">the event data.</param>
        public void OnDidRestore(OrmLifeCycleDidRestoreEvent didRestoreEvent);

        public record OrmLifeCycleEvent
        {
            public OrmSystem OrmSystem { get; init; }

            public OrmTransaction Transaction { get; init; }
        }

        public record OrmLifeCycleWillCreateEvent : OrmLifeCycleEvent
        {
        }

        public record OrmLifeCycleDidCreateEvent : OrmLifeCycleEvent
        {
            public OrmInstanceInfo CurrentInstance { get; init; }
        }

        public record OrmLifeCycleWillUpdateEvent : OrmLifeCycleEvent
        {
            public OrmInstanceInfo CurrentInstance { get; init; }
        }

        public record OrmLifeCycleDidUpdateEvent : OrmLifeCycleEvent
        {
            public OrmInstanceInfo CurrentInstance { get; init; }
        }

        public record OrmLifeCycleDidRestoreEvent : OrmLifeCycleEvent
        {
            public OrmInstanceInfo CurrentInstance { get; init; }
        }
    }
}
