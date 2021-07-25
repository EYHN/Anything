using System;

namespace Anything.FileSystem.Tracker
{
    /// <summary>
    ///     The file attached data.
    /// </summary>
    public record FileAttachedData
    {
        [Flags]
        public enum DeletionPolicies
        {
            WhenFileDeleted = 1,
            WhenFileContentChanged = 2
        }

        public FileAttachedData(string payload, DeletionPolicies deletionPolicy = DeletionPolicies.WhenFileDeleted)
        {
            Payload = payload;
            DeletionPolicy = deletionPolicy;
        }

        /// <summary>
        ///     Gets the payload of the data.
        /// </summary>
        public string Payload { get; }

        /// <summary>
        ///     Gets the policy on when to delete data.
        /// </summary>
        public DeletionPolicies DeletionPolicy { get; }
    }
}
