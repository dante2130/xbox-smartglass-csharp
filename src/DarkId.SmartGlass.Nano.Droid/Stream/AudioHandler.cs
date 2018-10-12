using System;
using System.Collections.Generic;
using Android.Media;
using DarkId.SmartGlass.Nano.Consumer;
using DarkId.SmartGlass.Nano.Packets;

namespace DarkId.SmartGlass.Nano.Droid
{
    public class AudioHandler
        : MediaCodec.Callback, Consumer.IAudioConsumer, IDisposable
    {
        private Packets.AudioFormat _audioFormat;

        private AudioTrack _audioTrack;
        private MediaCodec _audioCodec;

        private Queue<Consumer.AACFrame> _audioFrameQueue;

        public AudioHandler()
        {
            _audioFrameQueue = new Queue<Consumer.AACFrame>();
        }

        public void SetupAudio(int sampleRate, int channels, byte[] esdsData)
        {
            _audioTrack = new AudioTrack(
                    Stream.Music,
                    44100,
                    ChannelOut.Stereo,
                    Encoding.Pcm16bit,
                    4096,
                    AudioTrackMode.Stream);

            MediaFormat audioFormat = MediaFormat.CreateAudioFormat(
                mime: MediaFormat.MimetypeAudioAac,
                sampleRate: sampleRate,
                channelCount: channels);

            _audioCodec = MediaCodec.CreateDecoderByType(
                MediaFormat.MimetypeAudioAac);

            byte Profile = 1;
            byte sampleIndex = AacAdtsAssembler.GetSamplingFrequencyIndex(sampleRate);
            byte[] csd0 = new byte[2];
            csd0[0] = (byte)(((byte)Profile << 3) | (sampleIndex >> 1));
            csd0[1] = (byte)((byte)((sampleIndex << 7) & 0x80) | (channels << 3));

            esdsData = csd0;

            var esds = Java.Nio.ByteBuffer.Allocate(esdsData.Length).Put(esdsData);
            audioFormat.SetByteBuffer("csd-0", bytes: esds); // ESDS


            _audioCodec.SetCallback(this);
            _audioCodec.Configure(
                format: audioFormat,
                surface: null,
                crypto: null,
                flags: MediaCodecConfigFlags.None);

            _audioCodec.Start();
            _audioTrack.Play();
        }

        public void ConsumeAudioData(Packets.AudioData data)
        {
            AACFrame frame = AudioAssembler.AssembleAudioFrame(
                data,
                AACProfile.LC,
                (int)_audioFormat.SampleRate,
                (byte)_audioFormat.Channels);

            if (_audioCodec != null)
            {
                _audioFrameQueue.Enqueue(frame);
            }
        }

        public void ConsumeAudioFormat(Packets.AudioFormat format)
        {
            _audioFormat = format;
        }

        public override void OnError(MediaCodec codec, MediaCodec.CodecException e)
        {
            throw new NotImplementedException();
        }

        public override void OnInputBufferAvailable(MediaCodec codec, int index)
        {
            AACFrame frame;
            bool success = _audioFrameQueue.TryDequeue(out frame);

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
            _audioTrack.Write(decodedSample, 0, WriteMode.NonBlocking);

            codec.ReleaseOutputBuffer(index, true);
        }

        public override void OnOutputFormatChanged(MediaCodec codec, MediaFormat format)
        {
        }

        public void Dispose()
        {
            if (_audioCodec != null)
            {
                _audioCodec.Stop();
                _audioCodec.Release();
                _audioCodec.Dispose();
            }
            _audioFrameQueue.Clear();
        }
    }
}
