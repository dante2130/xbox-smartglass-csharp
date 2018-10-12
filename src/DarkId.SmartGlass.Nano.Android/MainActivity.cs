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
    public class MainActivity : Activity, TextureView.ISurfaceTextureListener
    {
        private bool setupRan = false;
        private TextureView _videoSurface;

        private static readonly string _hostname = "10.0.0.241";
        private SmartGlassClient _smartGlassClient;
        private NanoClient _nanoClient;
        private MediaCoreConsumer _mcConsumer;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            _videoSurface = FindViewById<TextureView>(Resource.Id.textureView1);
            _videoSurface.SurfaceTextureListener = this;
        }

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            if (setupRan)
                return;

            _mcConsumer = new MediaCoreConsumer(_videoSurface);
            Task.Run(() => StartStream());
            setupRan = true;
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

            _nanoClient = new NanoClient(_hostname, result.TcpPort, result.UdpPort, new System.Guid(), _mcConsumer);
            _nanoClient.StreamRunning += _nanoClient_StreamRunning;
            _nanoClient.Initialize();

            System.Diagnostics.Debug.WriteLine($"Nano connected and running.");
        }

        void _nanoClient_StreamRunning(object sender, System.EventArgs e)
        {
        }
    }
}

