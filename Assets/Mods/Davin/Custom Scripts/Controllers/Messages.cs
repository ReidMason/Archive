using UnityEngine;
using System.Collections;
using System;

using NoxCore.Placeables;
using NoxCore.Placeables.Ships;

namespace Davin.Messages
{
    public class RequestLocation : EventArgs
    {
        public Structure requester;

        public RequestLocation(Structure requester)
        {
            this.requester = requester;
        }
    }

    public class StructureLocation : EventArgs
    {
        public Structure structure;
        public Vector2 location;

        public StructureLocation(Structure structure, Vector2 location)
        {
            this.structure = structure;
            this.location = location;
        }
    }

    public class DistressCallMessage : EventArgs
    {
        public Structure distressed;

        public DistressCallMessage(Structure distressed)
        {
            this.distressed = distressed;
        }
    }

    public class NewPrimaryTargetMessage : EventArgs
    {
        public Structure target;

        public NewPrimaryTargetMessage(Structure target)
        {
            this.target = target;
        }
    }

    public class ReportingInMessage : EventArgs
    {
        public Ship ship;

        public ReportingInMessage(Ship ship)
        {
            this.ship = ship;
        }
    }

    public class BulkMessage : EventArgs
    {
        DistressCallMessage distressMessage;
        NewPrimaryTargetMessage primaryTargetMessage;

        public BulkMessage(DistressCallMessage distressMessage, NewPrimaryTargetMessage primaryTargetMessage)
        {
            this.distressMessage = distressMessage;
            this.primaryTargetMessage = primaryTargetMessage;
        }
    }
}