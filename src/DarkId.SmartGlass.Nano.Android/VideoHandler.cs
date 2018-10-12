using System;
using System.Collections.Generic;
using Android.Media;
using Android.Views;
using DarkId.SmartGlass.Nano.Consumer;
using DarkId.SmartGlass.Nano.Packets;

namespace DarkId.SmartGlass.Nano.Android
{
    public class VideoHandler : MediaCodec.Callback, Consumer.IVideoConsumer
    {
        private readonly TextureView _textureView;

        private Packets.VideoFormat _videoFormat;
        private Queue<Consumer.H264Frame> _videoFrameQueue;
        private Consumer.VideoAssembler _videoAssembler;

        private MediaCodec _videoCodec;

        public VideoHandler(TextureView textureView)
        {
            _textureView  = textureView;

            _videoFrameQueue = new Queue<Consumer.H264Frame>();
            _videoAssembler = new Consumer.VideoAssembler();
        }

        public void SetupVideo(int width, int height, byte[] spsData, byte[] ppsData)
        {
            MediaFormat videoFormat = MediaFormat.CreateVideoFormat(
                mime: MediaFormat.MimetypeVideoAvc,
                width: width,
                height: height);

            /*
             * TODO: Use SPS / PPS
            var sps = Java.Nio.ByteBuffer.Allocate(spsData.Length).Put(spsData);
            var pps = Java.Nio.ByteBuffer.Allocate(ppsData.Length).Put(ppsData);

            videoFormat.SetByteBuffer("csd-0", bytes: sps); // SPS
            videoFormat.SetByteBuffer("csd-1", bytes: pps); // PPS
            */

            videoFormat.SetInteger(MediaFormat.KeyMaxInputSize, 100000);

            _videoCodec = MediaCodec.CreateDecoderByType(
                                    MediaFormat.MimetypeVideoAvc);

            _videoCodec.SetCallback(this);
            _videoCodec.Configure(format: videoFormat,
                                  surface: new Surface(_textureView.SurfaceTexture),
                                  crypto: null,
                                  flags: MediaCodecConfigFlags.None);

            _videoCodec.Start();
        }

        public void ConsumeVideoData(VideoData data)
        {
            H264Frame frame = _videoAssembler.AssembleVideoFrame(data);
            if (frame != null)
            {

                /* TODO: Use this.. on main thread
                if (_videoCodec == null &&
                    (frame.PrimaryType == NalUnitType.SEQUENCE_PARAMETER_SET ||
                     frame.PrimaryType == NalUnitType.PICTURE_PARAMETER_SET))
                {
                    SetupVideo((int)_videoFormat.Width,
                                   (int)_videoFormat.Height,
                                   frame.GetSpsDataPrefixed(),
                                   frame.GetPpsDataPrefixed());
                }
                if (_videoCodec != null)
                {
                    _videoFrameQueue.Enqueue(frame);
                }
                */
                _videoFrameQueue.Enqueue(frame);
            }
        }

        public void ConsumeVideoFormat(VideoFormat format)
        {
            _videoFormat = format;
        }

        public override void OnError(MediaCodec codec, MediaCodec.CodecException e)
        {
            throw new NotImplementedException();
        }

        public override void OnInputBufferAvailable(MediaCodec codec, int index)
        {
            H264Frame frame;
            bool success = _videoFrameQueue.TryDequeue(out frame);

            if (!success)
            {
                codec.QueueInputBuffer(index, 0, 0, 0, 0);
                return;
            }

            Java.Nio.ByteBuffer buffer = codec.GetInputBuffer(index);
            buffer.Put(frame.RawData);

            // tell the decoder to process the frame
            codec.QueueInputBuffer(index, 0, frame.RawData.Length, 0, 0);
        }

        public override void OnOutputBufferAvailable(MediaCodec codec, int index, MediaCodec.BufferInfo info)
        {
            Java.Nio.ByteBuffer decodedSample = codec.GetOutputBuffer(index);

            // Just release outputBuffer, callback will handle rendering
            codec.ReleaseOutputBuffer(index, true);
        }

        public override void OnOutputFormatChanged(MediaCodec codec, MediaFormat format)
        {
        }
    }
}
