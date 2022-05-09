using System;
using System.Collections.Generic;

using NoxCore.Data.Fittings;
using NoxCore.Placeables;

namespace NoxCore.Fittings.Devices
{
    public interface IComms : IDevice
    {
        CommsData CommsData { get; set; }

        bool hasMessages();
        void removeMessage(EventArgs message);
        void clearMessages();
        List<EventArgs> getMessages();
        bool isSending();
        bool sendMessage(Structure recipient, EventArgs message);
        int broadcastMessage(List<Structure> recipients, EventArgs message);
        bool receiveMessage(EventArgs message);
    }
}