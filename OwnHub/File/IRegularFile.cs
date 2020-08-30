using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OwnHub.File
{
    public interface IRegularFile: IFile
    {
        public Stream Open();
    }
}
