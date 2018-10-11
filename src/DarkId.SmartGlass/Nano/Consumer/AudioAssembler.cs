using System;
using System.Collections.Generic;
using DarkId.SmartGlass.Nano.Packets;

namespace DarkId.SmartGlass.Nano.Consumer
{
    public static class AudioAssembler
    {
        public static AACFrame AssembleAudioFrame(AudioData data, AACProfile profile,
                                                int samplingFreq, byte channels)
        {
            return new AACFrame(data.Data, data.Timestamp, profile,
                                samplingFreq, channels);
        }
    }
}
