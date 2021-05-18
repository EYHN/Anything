using System;

namespace StagingBox.File.Base
{
    public abstract class BaseFileStats : IFileStats
    {
        public abstract long? Size { get; }

        public abstract DateTimeOffset? ModifyTime { get; }

        public abstract DateTimeOffset? AccessTime { get; }

        public abstract DateTimeOffset? CreationTime { get; }
    }
}
