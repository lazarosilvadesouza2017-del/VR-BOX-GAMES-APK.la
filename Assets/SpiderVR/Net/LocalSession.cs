using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace SpiderVR
{
    public class LocalSession : MonoBehaviour
    {
        public LanDiscovery discovery; public ushort port = 7777;
        public bool Host(string playerName)
        {
            var t = NetworkManager.Singleton.GetComponent<UnityTransport>();
            t.SetConnectionData("0.0.0.0", port, "0.0.0.0");
            discovery.StartAdvertising(playerName, port);
            return NetworkManager.Singleton.StartHost();
        }
        public bool Join(LanDiscovery.Host h)
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(h.ip, h.gamePort);
            return NetworkManager.Singleton.StartClient();
        }
    }
}
