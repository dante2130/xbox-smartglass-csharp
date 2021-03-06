using System;
using System.IO;
using DarkId.SmartGlass.Common;
using DarkId.SmartGlass.Nano;

namespace DarkId.SmartGlass.Nano.Packets
{
    [AudioPayloadType(AudioPayloadType.ClientHandshake)]
    internal class AudioClientHandshake : ISerializableLE
    {
        public uint InitialFrameID { get; private set; }
        public AudioFormat RequestedFormat { get; private set; }

        public AudioClientHandshake()
        {
            RequestedFormat = new AudioFormat();
        }
        
        public AudioClientHandshake(uint initialFrameID, AudioFormat requestedFormat)
        {
            InitialFrameID = initialFrameID;
            RequestedFormat = requestedFormat;
        }

        public void Deserialize(LEReader br)
        {
            InitialFrameID = br.ReadUInt32();
            RequestedFormat.Deserialize(br);
        }

        public void Serialize(LEWriter bw)
        {
            bw.Write(InitialFrameID);
            RequestedFormat.Serialize(bw);
        }
    }
}