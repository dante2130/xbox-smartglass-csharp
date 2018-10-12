
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Media;
using System.Threading.Tasks;
using DarkId.SmartGlass.Nano.Consumer;

namespace DarkId.SmartGlass.Nano.Droid
{
    [Activity(Label = "StreamActivity",
              ScreenOrientation = ScreenOrientation.Landscape)]
    public class StreamActivity : Activity, TextureView.ISurfaceTextureListener
    {
        private bool setupRan = false;
        private TextureView _videoSurface;

        private string _hostName;
        private SmartGlassClient _smartGlassClient;
        private NanoClient _nanoClient;
        private MediaCoreConsumer _mcConsumer;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //Remove title bar
            RequestWindowFeature(WindowFeatures.NoTitle);
            //Remove notification bar
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

            SetContentView(Resource.Layout.StreamLayout);

            _hostName = Intent.Extras.GetString("hostName");
            Toast.MakeText(this,
                           String.Format("Connecting to {0}...", _hostName),
                           ToastLength.Short)
                 .Show();

            // Create your application here
            _videoSurface = FindViewById<TextureView>(Resource.Id.tvVideoStream);
            _videoSurface.SurfaceTextureListener = this;
        }

        protected override void OnStop()
        {
            base.OnStop();

            _videoSurface.Dispose();
            _smartGlassClient.Dispose();
            _nanoClient.Dispose();
            _mcConsumer.Dispose();
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

            _smartGlassClient = await SmartGlassClient.ConnectAsync(_hostName);

            var broadcastChannel = await _smartGlassClient.GetBroadcastChannelAsync();
            var result = await broadcastChannel.StartGamestreamAsync();

            System.Diagnostics.Debug.WriteLine($"Connecting to Nano, TCP: {result.TcpPort}, UDP: {result.UdpPort}");

            _nanoClient = new NanoClient(_hostName,
                                         result.TcpPort, result.UdpPort,
                                         new System.Guid(),
                                         _mcConsumer);

            _nanoClient.StreamRunning += _nanoClient_StreamRunning;
            _nanoClient.Initialize();

            System.Diagnostics.Debug.WriteLine($"Nano connected and running.");
        }

        void _nanoClient_StreamRunning(object sender, System.EventArgs e)
        {
        }
    }
}
