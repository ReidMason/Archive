using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using NoxCore.Placeables;
using NoxCore.Data.Fittings;

namespace NoxCore.Fittings.Devices
{
    public class CommsSystem : Device, IComms
    {
        [Header("Comms System")]
        public CommsData commsData;
        protected CommsData _commsData;
        public CommsData CommsData { get { return _commsData; } set { _commsData = value; } }

        protected float commsTimer;
        protected bool sending;
        protected List<EventArgs> messages = new List<EventArgs>();

        public override void init(DeviceData deviceData = null)
        {
            if (deviceData == null)
            {
                CommsData = commsData;
                base.init(commsData);
            }
            else
            {
                CommsData = deviceData as CommsData;
                base.init(deviceData);
            }
        }

        public override void reset()
        {
            base.reset();

            clearMessages();
        }

        public bool hasMessages()
        {
            if (messages.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool isSending()
        {
            return sending;
        }

        public void removeMessage(EventArgs message)
        {
            messages.Remove(message);
        }

        public void clearMessages()
        {
            messages.Clear();
        }

        public List<EventArgs> getMessages()
        {
            return messages;
        }

        protected bool transmit(Structure recipient, EventArgs message)
        {
            // don't send to self
            if (recipient == structure) return false;

            // comms range check

            IComms[] comms = recipient.getDevices<IComms>().ToArray();

            foreach (IComms commsChannel in comms)
            {
                if (commsChannel.receiveMessage(message) == true)
                {
                    return true;
                }
            }

            return false;
        }

        public bool sendMessage(Structure recipient, EventArgs message)
        {
            if (sending == false)
            {
                sending = transmit(recipient, message);

                return sending;
            }

            return false;
        }

        public int broadcastMessage(List<Structure> recipients, EventArgs message)
        {
            if (sending == false)
            {
                int numReceivedOK = 0;

                foreach (Structure recipient in recipients)
                {
                    if (transmit(recipient, message) == true)
                    {
                        numReceivedOK++;
                    }
                }

                if (numReceivedOK > 0)
                {
                    sending = true;
                }
                else
                {
                    sending = false;
                }

                return numReceivedOK;
            }
            else
            {
                return 0;
            }
        }

        public bool receiveMessage(EventArgs message)
        {
            messages.Add(message);

            return true;
        }

        public override void update()
        {
            base.update();

            if (sending == true)
            {
                commsTimer += Time.deltaTime;

                if (commsTimer > commsData.RoundTrip)
                {
                    sending = false;
                    commsTimer = 0;
                }
            }
        }

        public static IComms DeviceToIComms(Device device)
        {
            return device as IComms;
        }
    }
}