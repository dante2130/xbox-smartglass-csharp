using Android.App;
using Android.Widget;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Graphics;
using Android.Media;
using System.Threading.Tasks;
using DarkId.SmartGlass.Nano.Consumer;
using DarkId.SmartGlass.Nano.Packets;
using System.Collections.Generic;

namespace DarkId.SmartGlass.Nano.Android
{
    [Activity(Label = "xNano", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity, TextureView.ISurfaceTextureListener, IConsumer
    {
        private Queue<byte[]> _videoFrameQueue;
        private VideoAssembler _videoAssembler;
        private TextureView _surface;
        private MediaCodec _mcodec;

        private static readonly string _hostname = "10.0.0.241";
        private SmartGlassClient _smartGlassClient;
        private NanoClient _nanoClient;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            _videoFrameQueue = new Queue<byte[]>();
            _videoAssembler = new VideoAssembler();

            _surface = FindViewById<TextureView>(Resource.Id.textureView1);
            _surface.SurfaceTextureListener = this;
        }

        public void ConsumeAudioData(AudioData data)
        {
        }

        public void ConsumeAudioFormat(Packets.AudioFormat format)
        {
        }

        public void ConsumeVideoData(VideoData data)
        {
            byte[] frame = _videoAssembler.AssembleVideoFrame(data);
            if (frame != null)
            {
                _videoFrameQueue.Enqueue(frame);
            }
        }

        public void ConsumeVideoFormat(VideoFormat format)
        {
            System.Diagnostics.Debug.WriteLine("Got VideoFormat: {0}", format);
        }

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            MediaFormat format = MediaFormat.CreateVideoFormat(
                mime: MediaFormat.MimetypeVideoAvc,
                width: 1280,
                height: 720);

            // TODO: Extract beforehand and set here
            //format.SetByteBuffer("csd-0", bytes: null); // SPS
            //format.SetByteBuffer("csd-1", bytes: null); // PPS

            format.SetInteger(MediaFormat.KeyMaxInputSize, 100000);

            _mcodec = MediaCodec.CreateDecoderByType(
                                            MediaFormat.MimetypeVideoAvc);

            _mcodec.Configure(format: format,
                              surface: new Surface(_surface.SurfaceTexture),
                              crypto: null,
                              flags: MediaCodecConfigFlags.None);

            _mcodec.Start();
            Task.Run(() => StartStream());
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

        public void VideoDecodeTask()
        {
            while (true)
            {
                byte[] frame;
                bool success =_videoFrameQueue.TryDequeue(out frame);

                if (!success)
                {
                    System.Threading.Thread.Sleep(200);
                    continue;
                }

                // Now we need to give it to the Codec to decode into the surface

                // Get the input buffer from the decoder
                // Pass in -1 here as in this example we don't have a playback time reference
                int inputIndex = _mcodec.DequeueInputBuffer(-1);

                // If  the buffer number is valid use the buffer with that index
                if (inputIndex >= 0)
                {
                    Java.Nio.ByteBuffer buffer = _mcodec.GetInputBuffer(inputIndex);
                    buffer.Put(frame);
                    // tell the decoder to process the frame
                    _mcodec.QueueInputBuffer(inputIndex, 0, frame.Length, 0, 0);
                }

                MediaCodec.BufferInfo info = new MediaCodec.BufferInfo();
                int outputIndex = _mcodec.DequeueOutputBuffer(info, 0);
                if (outputIndex >= 0)
                {
                    _mcodec.ReleaseOutputBuffer(outputIndex, true);
                }
            }
        }
    }
}

