using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AppKit;
using DarkId.SmartGlass.Nano.AVFoundation;
using Foundation;

namespace DarkId.SmartGlass.Nano.macOS
{
    public class AVPlayer : NSObject
    {
        private static readonly string _hostname = "10.0.0.241";
        private SmartGlassClient _smartGlassClient;
        private NanoClient _nanoClient;
        private AVFoundationConsumer _avConsumer;

        public AVPlayer() : base()
        {
            _avConsumer = new AVFoundationConsumer();
        }

        public void SetView(NSView view)
        {
            _avConsumer.SetView(view);
        }

        public async Task CreateClient()
        {
            Debug.WriteLine($"Connecting to console...");

            _smartGlassClient = await SmartGlassClient.ConnectAsync(_hostname);

            var broadcastChannel = await _smartGlassClient.GetBroadcastChannelAsync();
            var result = await broadcastChannel.StartGamestreamAsync();

            Debug.WriteLine($"Connecting to Nano, TCP: {result.TcpPort}, UDP: {result.UdpPort}");

            _nanoClient = new NanoClient(_hostname, result.TcpPort, result.UdpPort, new Guid(), _avConsumer);
            _nanoClient.Initialize();

            Debug.WriteLine($"Nano connected and running.");
        }
    }
}
