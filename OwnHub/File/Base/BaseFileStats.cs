using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.File.Base
{
    public abstract class BaseFileStats : IFileStats
    {
        public abstract long? Size { get; }

        public abstract DateTimeOffset? ModifyTime { get; }

        public abstract DateTimeOffset? AccessTime { get; }

        public abstract DateTimeOffset? CreationTime { get; }
    }
}
