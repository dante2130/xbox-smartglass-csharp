using System;
using DarkId.SmartGlass.Nano;
using DarkId.SmartGlass.Nano.Packets;

namespace DarkId.SmartGlass.Nano.Channels
{
    internal class InputChannel : InputChannelBase
    {
        public bool HandshakeDone { get; internal set; }

        public InputChannel(NanoClient client)
            : base(client, NanoChannelId.Input)
        {
            HandshakeDone = false;
        }

        public override void OnClientHandshake(InputClientHandshake handshake)
        {
        }

        public override void OnServerHandshake(InputServerHandshake handshake)
        {
        }

        public override void OnFrame(InputFrame frame)
        {
            throw new NotSupportedException("Input frame on client side");
        }

        public override void OnFrameAck(InputFrameAck ack)
        {
        }
    }
}