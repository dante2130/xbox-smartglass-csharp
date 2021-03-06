﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DarkId.SmartGlass.Channels.Broadcast.Messages
{
    class GamestreamStartMessage : BroadcastBaseMessage
    {
        public GamestreamConfiguration Configuration { get; set; }
        public bool ReQueryPreviewStatus { get; set; }

        public GamestreamStartMessage()
        {
            Type = BroadcastMessageType.StartGamestream;
        }
    }
}
