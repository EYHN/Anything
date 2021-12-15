using System.Threading.Tasks;
using MessagePack;

namespace Anything.FileSystem.Property;

public static class PropertyOperationsExtensions
{
    public static async ValueTask AddOrUpdateObjectProperty<T>(
        this IPropertyOperations fs,
        FileHandle fileHandle,
        string name,
        T value,
        PropertyFeature feature = PropertyFeature.None)
        where T : class
    {
        var data = MessagePackSerializer.Serialize(value);
        await fs.SetProperty(fileHandle, name, data, feature);
    }

    public static async ValueTask<T?> GetObjectProperty<T>(
        this IPropertyOperations fs,
        FileHandle fileHandle,
        string name)
        where T : class
    {
        var data = await fs.GetProperty(fileHandle, name);
        if (data == null)
        {
            return null;
        }

        return MessagePackSerializer.Deserialize<T>(data.Value);
    }
}
