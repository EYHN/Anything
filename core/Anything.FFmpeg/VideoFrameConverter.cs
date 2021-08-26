using System;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace Anything.FFmpeg
{
    public sealed unsafe class VideoFrameConverter : IDisposable
    {
        private readonly IntPtr _convertedFrameBufferPtr;
        private readonly int _destinationHeight;
        private readonly int _destinationWidth;
        private readonly byte_ptrArray4 _dstData;
        private readonly int_array4 _dstLinesize;
        private readonly SwsContext* _pConvertContext;

        public VideoFrameConverter(
            int sourceWidth,
            int sourceHeight,
            AVPixelFormat sourcePixelFormat,
            int destinationWidth,
            int destinationHeight,
            AVPixelFormat destinationPixelFormat = AVPixelFormat.AV_PIX_FMT_RGBA)
        {
            _destinationWidth = destinationWidth;
            _destinationHeight = destinationHeight;

            _pConvertContext = ffmpeg.sws_getContext(
                sourceWidth,
                sourceHeight,
                sourcePixelFormat,
                destinationWidth,
                destinationHeight,
                destinationPixelFormat,
                ffmpeg.SWS_FAST_BILINEAR,
                null,
                null,
                null);
            if (_pConvertContext == null)
            {
                throw new FFmpegException("Could not initialize the conversion context.");
            }

            var convertedFrameBufferSize = ffmpeg.av_image_get_buffer_size(
                destinationPixelFormat,
                destinationWidth,
                destinationHeight,
                1);
            _convertedFrameBufferPtr = Marshal.AllocHGlobal(convertedFrameBufferSize);
            _dstData = default;
            _dstLinesize = default;

            ffmpeg.av_image_fill_arrays(
                ref _dstData,
                ref _dstLinesize,
                (byte*)_convertedFrameBufferPtr,
                destinationPixelFormat,
                destinationWidth,
                destinationHeight,
                1);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(_convertedFrameBufferPtr);
            ffmpeg.sws_freeContext(_pConvertContext);
        }

        public AVFrame Convert(AVFrame sourceFrame)
        {
            ffmpeg.sws_scale(
                _pConvertContext,
                sourceFrame.data,
                sourceFrame.linesize,
                0,
                sourceFrame.height,
                _dstData,
                _dstLinesize);

            var data = default(byte_ptrArray8);
            data.UpdateFrom(_dstData);
            var lineSize = default(int_array8);
            lineSize.UpdateFrom(_dstLinesize);

            return new AVFrame { data = data, linesize = lineSize, width = _destinationWidth, height = _destinationHeight };
        }
    }
}
