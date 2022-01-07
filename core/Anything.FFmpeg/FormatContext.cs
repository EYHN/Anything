using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using Nito.Disposables;

namespace Anything.FFmpeg;

public unsafe class FormatContext : SingleDisposable<object?>
{
    private const int ContextBufferSize = 4096;

    public AVFormatContext* RawFormatContext { get; }

    private readonly AVIOContext* _rawIoContext;

    private readonly Stream _stream;

    private GCHandle _handle;

    private byte[] _managedContextBuffer;

    public long BitRate => RawFormatContext->bit_rate;

    /// <summary>
    /// Gets duration of the file, in seconds.
    /// </summary>
    public double Duration => (double)RawFormatContext->duration / ffmpeg.AV_TIME_BASE;

    public FormatContext(Stream stream)
        : base(null)
    {
        _stream = stream;
        var formatContext = ffmpeg.avformat_alloc_context();

        _managedContextBuffer = new byte[ContextBufferSize];
        _handle = GCHandle.Alloc(this, GCHandleType.Normal);

        // gets freed by libavformat when closing the input
        var contextBuffer = (byte*)ffmpeg.av_malloc(ContextBufferSize);

        _rawIoContext = ffmpeg.avio_alloc_context(
            contextBuffer,
            ContextBufferSize,
            0,
            (void*)GCHandle.ToIntPtr(_handle),
            _readPacketCallback,
            null,
            _seekCallback);
        formatContext->pb = _rawIoContext;

        ffmpeg.avformat_open_input(&formatContext, "<no file>", null, null).ThrowExceptionIfError();
        ffmpeg.avformat_find_stream_info(formatContext, null).ThrowExceptionIfError();

        RawFormatContext = formatContext;
    }

    public Dictionary<string, string> ReadMetadata()
    {
        var metadata = new Dictionary<string, string>();

        AVDictionaryEntry* tag = null;
        while (true)
        {
            tag = ffmpeg.av_dict_get(RawFormatContext->metadata, "", tag, ffmpeg.AV_DICT_IGNORE_SUFFIX);
            if (tag == null)
            {
                break;
            }

            metadata.Add(
                Marshal.PtrToStringUTF8((IntPtr)tag->key)!.ToLower(CultureInfo.InvariantCulture),
                Marshal.PtrToStringUTF8((IntPtr)tag->value)!);
        }

        return metadata;
    }

    public MediaStream? FindBestVideoStream()
    {
        return FindBestStream(AVMediaType.AVMEDIA_TYPE_VIDEO);
    }

    public MediaStream? FindBestAudioStream()
    {
        return FindBestStream(AVMediaType.AVMEDIA_TYPE_AUDIO);
    }

    public MediaStream? FindAttachedPicStream()
    {
        for (var i = 0; i < RawFormatContext->nb_streams; i++)
        {
            if ((RawFormatContext->streams[i]->disposition & ffmpeg.AV_DISPOSITION_ATTACHED_PIC) != 0)
            {
                return new MediaStream(RawFormatContext, i, RawFormatContext->streams[i]);
            }
        }

        return null;
    }

    private MediaStream? FindBestStream(AVMediaType mediaType)
    {
        AVCodec* decoder;
        var streamIndex = ffmpeg.av_find_best_stream(RawFormatContext, mediaType, -1, -1, &decoder, 0);
        if (streamIndex < 0)
        {
            return null;
        }

        return new MediaStream(RawFormatContext, streamIndex, RawFormatContext->streams[streamIndex], decoder);
    }

    private static readonly avio_alloc_context_read_packet _readPacketCallback = ReadPacketCallback;

    private static readonly avio_alloc_context_seek _seekCallback = SeekCallback;

    private static int ReadPacketCallback(void* opaque, byte* bufferPtr, int bufferSize)
    {
        var handle = GCHandle.FromIntPtr((IntPtr)opaque);
        if (!handle.IsAllocated || handle.Target is not FormatContext decoder)
        {
            return 0;
        }

        if (bufferSize != decoder._managedContextBuffer.Length)
        {
            decoder._managedContextBuffer = new byte[bufferSize];
        }

        var bytesRead = decoder._stream.Read(decoder._managedContextBuffer, 0, bufferSize);
        if (bytesRead == 0)
        {
            return ffmpeg.AVERROR_EOF;
        }

        Marshal.Copy(decoder._managedContextBuffer, 0, (IntPtr)bufferPtr, bytesRead);
        return bytesRead;
    }

    private static long SeekCallback(void* opaque, long offset, int whence)
    {
        var handle = GCHandle.FromIntPtr((IntPtr)opaque);
        if (!handle.IsAllocated || handle.Target is not FormatContext decoder)
        {
            return -1;
        }

        if (!decoder._stream.CanSeek)
        {
            throw new InvalidOperationException("Tried seeking on a file sourced by a non-seekable stream.");
        }

        switch (whence)
        {
            case StdIo.SeekCur:
                decoder._stream.Seek(offset, SeekOrigin.Current);
                break;

            case StdIo.SeekEnd:
                decoder._stream.Seek(offset, SeekOrigin.End);
                break;

            case StdIo.SeekSet:
                decoder._stream.Seek(offset, SeekOrigin.Begin);
                break;

            case ffmpeg.AVSEEK_SIZE:
                return decoder._stream.Length;

            default:
                return -1;
        }

        return decoder._stream.Position;
    }

    protected override void Dispose(object? context)
    {
        var pFormatContext = RawFormatContext;
        ffmpeg.avformat_close_input(&pFormatContext);

        var pIoContext = _rawIoContext;
        ffmpeg.av_freep(&pIoContext->buffer);
        ffmpeg.avio_context_free(&pIoContext);

        _handle.Free();
    }

    private static class StdIo
    {
        internal const int SeekSet = 0;

        internal const int SeekCur = 1;

        internal const int SeekEnd = 2;
    }
}
