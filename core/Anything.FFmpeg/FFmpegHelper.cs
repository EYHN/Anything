using System;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace Anything.FFmpeg;

public static class FFmpegHelper
{
    internal static unsafe string? AvErrorToStr(int error)
    {
        var bufferSize = 1024;
        var buffer = stackalloc byte[bufferSize];
        ffmpeg.av_strerror(error, buffer, (ulong)bufferSize);
        var message = Marshal.PtrToStringAnsi((IntPtr)buffer);
        return message;
    }

    internal static int ThrowExceptionIfError(this int error)
    {
        if (error < 0)
        {
            throw new FFmpegException(AvErrorToStr(error));
        }

        return error;
    }

    internal static double GetValue(this AVRational rational)
    {
        return rational.num / (double)rational.den;
    }

    public static void SetupFFmpegLibraryLoader()
    {
        ffmpeg.GetOrLoadLibrary = name =>
        {
            var version = ffmpeg.LibraryVersionMap[name];

            string libraryName;

            // "lib" prefix and extensions are resolved by .net core
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                libraryName = $"{name}-{version}";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                libraryName = $"{name}.{version}";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                libraryName = $"{name}.so.{version}";
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            return NativeLibrary.Load(
                libraryName,
                typeof(FFmpegHelper).Assembly,
                DllImportSearchPath.UseDllDirectoryForDependencies | DllImportSearchPath.SafeDirectories);
        };
    }
}
