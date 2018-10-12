using System;
using System.Collections.Generic;
using Android.Media;
using Android.Views;
using DarkId.SmartGlass.Nano.Consumer;
using DarkId.SmartGlass.Nano.Packets;

namespace DarkId.SmartGlass.Nano.Android
{
    public class MediaCoreConsumer : IConsumer
    {
        VideoHandler _video;
        AudioHandler _audio;

        public MediaCoreConsumer(TextureView textureView)
        {
            _video = new VideoHandler(textureView);
            _audio = new AudioHandler();
            //TODO: Setup dynamically
            _video.SetupVideo(1280, 720, null, null);
        }

        public void ConsumeAudioData(AudioData data)
        {
            _audio.ConsumeAudioData(data);
        }

        public void ConsumeAudioFormat(Packets.AudioFormat format)
        {
            _audio.ConsumeAudioFormat(format);
        }

        public void ConsumeVideoData(VideoData data)
        {
            _video.ConsumeVideoData(data);
        }

        public void ConsumeVideoFormat(VideoFormat format)
        {
            _video.ConsumeVideoFormat(format);
        }

        public void Dispose()
        {
            _video.Dispose();
            _audio.Dispose();
        }
    }
}
