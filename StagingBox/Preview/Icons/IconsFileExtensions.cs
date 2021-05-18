using StagingBox.File;

namespace StagingBox.Preview.Icons
{
    public static class IconsFileExtensions
    {
        public static string GetIcon(this IFile file)
        {
            if (file is IRegularFile)
            {
                MimeType? mime = file.MimeType;
                return mime?.Icon ?? "regular_file";
            }

            if (file is IDirectory) return "directory";

            return "unknown_file";
        }
    }
}
