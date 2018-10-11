using Android.App;
using Android.Widget;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Graphics;
using Android.Media;
using System.Threading.Tasks;
using DarkId.SmartGlass.Nano.Consumer;
using System.Collections.Generic;

namespace DarkId.SmartGlass.Nano.Android
{
    [Activity(Label = "xNano", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity, TextureView.ISurfaceTextureListener, IConsumer
    {
        private Queue<H264Frame> _videoFrameQueue;
        private VideoAssembler _videoAssembler;
        private TextureView _videoSurface;
        private MediaCodec _videoCodec;

        private Queue<byte[]> _audioFrameQueue;
        private AudioTrack _audioTrack;
        private MediaCodec _audioCodec;

        private static readonly string _hostname = "10.0.0.241";
        private SmartGlassClient _smartGlassClient;
        private NanoClient _nanoClient;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            _videoFrameQueue = new Queue<H264Frame>();
            _videoAssembler = new VideoAssembler();

            _videoSurface = FindViewById<TextureView>(Resource.Id.textureView1);
            _videoSurface.SurfaceTextureListener = this;
        }

        public void ConsumeAudioData(Packets.AudioData data)
        {
        }

        public void ConsumeAudioFormat(Packets.AudioFormat format)
        {
        }

        public void ConsumeVideoData(Packets.VideoData data)
        {
            H264Frame frame = _videoAssembler.AssembleVideoFrame(data);
            if (frame != null)
            {
                _videoFrameQueue.Enqueue(frame);
            }
        }

        public void ConsumeVideoFormat(Packets.VideoFormat format)
        {
            System.Diagnostics.Debug.WriteLine("Got VideoFormat: {0}", format);
        }

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            return false;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
        }

        public async Task StartStream()
        {
            System.Diagnostics.Debug.WriteLine($"Connecting to console...");

            _smartGlassClient = await SmartGlassClient.ConnectAsync(_hostname);

            var broadcastChannel = await _smartGlassClient.GetBroadcastChannelAsync();
            var result = await broadcastChannel.StartGamestreamAsync();

            System.Diagnostics.Debug.WriteLine($"Connecting to Nano, TCP: {result.TcpPort}, UDP: {result.UdpPort}");

            _nanoClient = new NanoClient(_hostname, result.TcpPort, result.UdpPort, new System.Guid(), this);
            _nanoClient.Initialize();

            System.Diagnostics.Debug.WriteLine($"Nano connected and running.");

            VideoDecodeTask();
        }

        public byte[] GetAudioCodecSpecificData(AACProfile profile,
                                                int sampleRate,
                                                int channels)
        {
            //TODO: Get sampleIndex from DarkId.SmartGlass
            byte sampleIndex = 0;
            byte[] csd0 = new byte[2];
            csd0[0] = (byte)(((byte)profile << 3) | (sampleIndex >> 1));
            csd0[1] = (byte)((byte)((sampleIndex << 7) & 0x80) | (channels << 3));

            return csd0;
        }

        public void SetupAudio()
        {
            //TODO: Setup _audioTrack
            _audioTrack = new AudioTrack();

            //TODO: Get values from AudioFormat
            MediaFormat audioFormat = MediaFormat.CreateAudioFormat(
                mime: MediaFormat.MimetypeAudioAac,
                sampleRate: 38400,
                channelCount: 2);

            _audioCodec = MediaCodec.CreateDecoderByType(
                MediaFormat.MimetypeAudioAac);

            _audioCodec.Configure(
                format: audioFormat,
                surface: null,
                crypto: null,
                flags: MediaCodecConfigFlags.None);

            _audioCodec.Start();
        }

        public void SetupVideo()
        {
            //TODO: Get values from VideoFormat
            MediaFormat videoFormat = MediaFormat.CreateVideoFormat(
                mime: MediaFormat.MimetypeVideoAvc,
                width: 1280,
                height: 720);

            // TODO: Extract beforehand and set here
            //format.SetByteBuffer("csd-0", bytes: null); // SPS
            //format.SetByteBuffer("csd-1", bytes: null); // PPS

            videoFormat.SetInteger(MediaFormat.KeyMaxInputSize, 100000);

            _videoCodec = MediaCodec.CreateDecoderByType(
                                            MediaFormat.MimetypeVideoAvc);

            _videoCodec.Configure(format: videoFormat,
                                  surface: new Surface(_videoSurface.SurfaceTexture),
                                  crypto: null,
                                  flags: MediaCodecConfigFlags.None);

            _videoCodec.Start();
        }

        public void AudioDecodeTask()
        {
            while (true)
            {
                byte[] frame;
                bool success = _audioFrameQueue.TryDequeue(out frame);

                if (!success)
                {
                    continue;
                }

                // Now we need to give it to the Codec to decode

                // Get the input buffer from the decoder
                // Pass in -1 here as in this example we don't have a playback time reference
                int inputIndex = _audioCodec.DequeueInputBuffer(-1);

                // If  the buffer number is valid use the buffer with that index
                if (inputIndex >= 0)
                {
                    Java.Nio.ByteBuffer buffer = _audioCodec.GetInputBuffer(inputIndex);
                    buffer.Put(frame);
                    // tell the decoder to process the frame
                    _audioCodec.QueueInputBuffer(inputIndex, 0, frame.Length, 0, 0);
                }

                MediaCodec.BufferInfo info = new MediaCodec.BufferInfo();
                int outputIndex = _audioCodec.DequeueOutputBuffer(info, 0);
                if (outputIndex >= 0)
                {
                    Java.Nio.ByteBuffer decodedSample =_audioCodec.GetOutputBuffer(outputIndex);
                    //TODO: Send decoded to audioTrack to play
                    _audioTrack.Write();
                    _audioTrack.Play();

                    // Just release outputBuffer, callback will handle rendering
                    _videoCodec.ReleaseOutputBuffer(outputIndex, true);
                }
            }
        }

        public void VideoDecodeTask()
        {
            while (true)
            {
                H264Frame frame;
                bool success =_videoFrameQueue.TryDequeue(out frame);

                if (!success)
                {
                    continue;
                }

                // Now we need to give it to the Codec to decode into the surface

                // Get the input buffer from the decoder
                // Pass in -1 here as in this example we don't have a playback time reference
                int inputIndex = _videoCodec.DequeueInputBuffer(-1);

                // If  the buffer number is valid use the buffer with that index
                if (inputIndex >= 0)
                {
                    //TODO: Check if this is the expected data layout
                    byte[] frameData = frame.RawData;

                    Java.Nio.ByteBuffer buffer = _videoCodec.GetInputBuffer(inputIndex);
                    buffer.Put(frameData);
                    // tell the decoder to process the frame
                    _videoCodec.QueueInputBuffer(inputIndex, 0, frameData.Length, 0, 0);
                }

                MediaCodec.BufferInfo info = new MediaCodec.BufferInfo();
                int outputIndex = _videoCodec.DequeueOutputBuffer(info, 0);
                if (outputIndex >= 0)
                {
                    // Just release outputBuffer, callback will handle rendering
                    _videoCodec.ReleaseOutputBuffer(outputIndex, true);
                }
            }
        }
    }
}

