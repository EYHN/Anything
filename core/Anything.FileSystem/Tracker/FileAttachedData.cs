using System;

namespace Anything.FileSystem.Tracker
{
    /// <summary>
    ///     The file attached data.
    /// </summary>
    public record FileAttachedData
    {
        public FileAttachedData(string payload)
        {
            Payload = payload;
        }

        /// <summary>
        ///     Gets the payload of the data.
        /// </summary>
        public string Payload { get; }
    }
}
