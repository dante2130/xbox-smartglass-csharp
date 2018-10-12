using System;
using System.Collections.Generic;
using Android.Media;
using DarkId.SmartGlass.Nano.Consumer;
using DarkId.SmartGlass.Nano.Packets;

namespace DarkId.SmartGlass.Nano.Android
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

        public void SetupAudio(int sampleRate, int channels)
        {
            //TODO: Setup _audioTrack
            // _audioTrack = new AudioTrack();

            MediaFormat audioFormat = MediaFormat.CreateAudioFormat(
                mime: MediaFormat.MimetypeAudioAac,
                sampleRate: sampleRate,
                channelCount: channels);

            _audioCodec = MediaCodec.CreateDecoderByType(
                MediaFormat.MimetypeAudioAac);

            _audioCodec.SetCallback(this);
            _audioCodec.Configure(
                format: audioFormat,
                surface: null,
                crypto: null,
                flags: MediaCodecConfigFlags.None);

            _audioCodec.Start();
        }

        public void ConsumeAudioData(Packets.AudioData data)
        {
            return;
            AACFrame frame = AudioAssembler.AssembleAudioFrame(
                data,
               AACProfile.LC,
               (int)_audioFormat.SampleRate,
               (byte)_audioFormat.Channels);
            if (_audioCodec == null)
            {
                SetupAudio(frame.SampleRate, frame.Channels);
            }
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
                return;

            Java.Nio.ByteBuffer buffer = codec.GetInputBuffer(index);
            buffer.Put(frame.RawData);
            buffer.Flip();

            // tell the decoder to process the frame
            codec.QueueInputBuffer(index, 0, frame.RawData.Length, 0, 0);
        }

        public override void OnOutputBufferAvailable(MediaCodec codec, int index, MediaCodec.BufferInfo info)
        {
            Java.Nio.ByteBuffer decodedSample = codec.GetOutputBuffer(index);
            //TODO: Send decoded to audioTrack to play
            //_audioTrack.Write();
            //_audioTrack.Play();

            codec.ReleaseOutputBuffer(index, true);
        }

        public override void OnOutputFormatChanged(MediaCodec codec, MediaFormat format)
        {
            throw new NotImplementedException();
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
