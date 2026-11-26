using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace SpiderVR
{
    /// <summary>Multiplayer LOCAL: quem hospeda anuncia por UDP broadcast; quem procura lista os anfitrioes da mesma rede Wi-Fi.</summary>
    public class LanDiscovery : MonoBehaviour
    {
        public const int Port = 47777;
        public class Host { public string name, ip; public ushort gamePort; public float lastSeen; }
        public Dictionary<string, Host> Found = new Dictionary<string, Host>();
        UdpClient listener, sender; float nextBeacon; string beacon; bool advertising;

        public void StartListening()
        {
            StopListening();
            listener = new UdpClient(Port) { EnableBroadcast = true };
            listener.Client.Blocking = false;
        }
        public void StopListening() { listener?.Close(); listener = null; }

        public void StartAdvertising(string playerName, ushort gamePort)
        {
            sender = new UdpClient { EnableBroadcast = true };
            beacon = "SPIDERVR|" + playerName + "|" + gamePort; advertising = true;
        }
        public void StopAdvertising() { advertising = false; sender?.Close(); sender = null; }

        void Update()
        {
            if (advertising && Time.unscaledTime > nextBeacon)
            {
                nextBeacon = Time.unscaledTime + 1f;
                var b = Encoding.UTF8.GetBytes(beacon);
                try { sender.Send(b, b.Length, new IPEndPoint(IPAddress.Broadcast, Port)); } catch { }
            }
            while (listener != null && listener.Available > 0)
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                string msg = Encoding.UTF8.GetString(listener.Receive(ref ep));
                var p = msg.Split('|');
                if (p.Length == 3 && p[0] == "SPIDERVR" && ushort.TryParse(p[2], out var gp))
                    Found[ep.Address.ToString()] = new Host { name = p[1], ip = ep.Address.ToString(), gamePort = gp, lastSeen = Time.unscaledTime };
            }
            var old = new List<string>();
            foreach (var kv in Found) if (Time.unscaledTime - kv.Value.lastSeen > 4f) old.Add(kv.Key);
            foreach (var k in old) Found.Remove(k);
        }
        void OnDestroy() { StopListening(); StopAdvertising(); }
    }
}
