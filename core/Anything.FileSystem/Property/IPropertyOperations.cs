using System;
using System.Threading.Tasks;

namespace Anything.FileSystem.Property;

public interface IPropertyOperations
{
    public ValueTask SetProperty(
        FileHandle fileHandle,
        string name,
        ReadOnlyMemory<byte> value,
        PropertyFeature feature = PropertyFeature.None);

    public ValueTask<ReadOnlyMemory<byte>?> GetProperty(FileHandle fileHandle, string name);

    public ValueTask RemoveProperty(FileHandle fileHandle, string name);
}
