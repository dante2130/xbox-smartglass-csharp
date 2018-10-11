using System;
using DarkId.SmartGlass.Nano.Packets;
using AV = AVFoundation;
using AT = AudioToolbox;

namespace DarkId.SmartGlass.Nano.AVFoundation
{
    public static class PacketExtensions
    {                                 
        public static AV.AVAudioFormat ToAVAudioFormat(this AudioFormat format)
        {
            var settings = format.ToATAudioStreamDescription();
            return new AV.AVAudioFormat(ref settings);
        }

        public static AT.AudioStreamBasicDescription ToATAudioStreamDescription(this AudioFormat format)
        {
            return format.Codec == AudioCodec.PCM
                ? new AT.AudioStreamBasicDescription()
                {
                    Format = format.Codec.ToATFormatType(),
                    ChannelsPerFrame = (int)format.Channels,
                    SampleRate = format.SampleRate,
                    FormatFlags = AT.AudioFormatFlags.LinearPCMIsFloat,
                    FramesPerPacket = 1,
                    BitsPerChannel = (int)format.SampleSize / 2 / 8,
                    BytesPerFrame = (int)format.SampleSize,
                    BytesPerPacket = (int)format.SampleSize,
                }
                : new AT.AudioStreamBasicDescription()
                {
                    Format = format.Codec.ToATFormatType(),
                    ChannelsPerFrame = (int)format.Channels,
                    SampleRate = format.SampleRate,
                    FramesPerPacket = 1,
                    BitsPerChannel = 16,
                    BytesPerFrame = 420,
                    BytesPerPacket = 420
                };
        }

        public static AT.AudioFormatType ToATFormatType(this AudioCodec codec)
        {
            switch (codec)
            {
                case AudioCodec.AAC:
                    return AT.AudioFormatType.MPEG4AAC;
                case AudioCodec.PCM:
                    return AT.AudioFormatType.LinearPCM;
                case AudioCodec.Opus:
                    return AT.AudioFormatType.Opus;
                default:
                    throw new SmartGlassException("Unknown audio codec type.");
            }
        }
    }
}
