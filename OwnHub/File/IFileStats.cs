using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.File
{
    public interface IFileStats
    {
        /// <summary>
        /// The size of the file in bytes.
        /// </summary>
        public long? Size { get; }

        /// <summary>
        /// The last time this file was modified.
        /// </summary>
        public DateTimeOffset? ModifyTime { get; }

        /// <summary>
        /// The last time this file was accessed.
        /// </summary>
        public DateTimeOffset? AccessTime { get; }

        /// <summary>
        /// The creation time of the file.
        /// </summary>
        public DateTimeOffset? CreationTime { get; }
    }
}
