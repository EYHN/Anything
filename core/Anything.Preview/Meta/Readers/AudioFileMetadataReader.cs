using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Anything.FFmpeg;
using Anything.FileSystem;
using Anything.Preview.Meta.Schema;

namespace Anything.Preview.Meta.Readers;

public class AudioFileMetadataReader : IMetadataReader
{
    private readonly IFileService _fileService;

    public AudioFileMetadataReader(IFileService fileService)
    {
        _fileService = fileService;
        FFmpegHelper.SetupFFmpegLibraryLoader();
    }

    public bool IsSupported(MetadataReaderFileInfo fileInfo)
    {
        if (fileInfo.Type.HasFlag(FileType.File) && fileInfo.MimeType != null &&
            fileInfo.MimeType.Mime.StartsWith("audio/", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }

    public async Task<Metadata> ReadMetadata(Metadata metadata, MetadataReaderFileInfo fileInfo, MetadataReaderOption option)
    {
        return await _fileService.ReadFileStream(fileInfo.FileHandle, videoStream =>
        {
            using var formatContext = new FormatContext(videoStream);

            var duration = formatContext.Duration;
            metadata.Information.Duration = TimeSpan.FromSeconds(duration);

            var tags = formatContext.ReadMetadata();

            return ReadTagMetadata(metadata, tags);
        });
    }

    private static Metadata ReadTagMetadata(Metadata metadata, Dictionary<string, string> tags)
    {
        TryGetStringTagToRef(tags, ref metadata.Music.Title, "title", "sort_name");
        TryGetStringTagToRef(tags, ref metadata.Music.Artist, "artist", "sort_artist");
        TryGetStringTagToRef(tags, ref metadata.Music.Album, "album", "sort_album");
        TryGetStringTagToRef(tags, ref metadata.Music.Genre, "genre");
        TryGetStringTagToRef(tags, ref metadata.Music.Date, "date");
        TryGetStringTagToRef(tags, ref metadata.Music.Composer, "composer", "sort_composer");
        TryGetStringTagToRef(tags, ref metadata.Music.AlbumArtist, "album_artist", "sort_album_artist");

        // Track Num/Total
        if (tags.TryGetValue("track", out var trackStr))
        {
            string track;
            string? trackTotal;
            var trackMatch = new Regex("(?<track>\\d+)/(?<trackTotal>\\d+)").Match(trackStr);
            if (trackMatch.Success)
            {
                track = trackMatch.Groups["track"].Value;
                trackTotal = trackMatch.Groups["trackTotal"].Value;
            }
            else
            {
                track = trackStr;
                trackTotal = "";
            }

            TryGetStringTagToRef(tags, ref trackTotal, "tracktotal");
            metadata.Music.Track = track + (string.IsNullOrWhiteSpace(trackTotal) ? "" : "/" + trackTotal);
        }

        // Disc Num/Total
        if (tags.TryGetValue("disc", out var discStr))
        {
            string disc;
            string? discTotal;
            var discMatch = new Regex("(?<disc>\\d+)/(?<discTotal>\\d+)").Match(discStr);
            if (discMatch.Success)
            {
                disc = discMatch.Groups["disc"].Value;
                discTotal = discMatch.Groups["discTotal"].Value;
            }
            else
            {
                disc = discStr;
                discTotal = "";
            }

            TryGetStringTagToRef(tags, ref discTotal, "disctotal");
            metadata.Music.Disc = disc + (string.IsNullOrWhiteSpace(discTotal) ? "" : "/" + discTotal);
        }

        return metadata;
    }

    private static void TryGetStringTagToRef(Dictionary<string, string> tags, ref string? metadata, params string[] tagName)
    {
        foreach (var tag in tagName)
        {
            if (tags.TryGetValue(tag, out var tagValue) && !string.IsNullOrWhiteSpace(tagValue))
            {
                metadata = tagValue;
                return;
            }
        }
    }
}
