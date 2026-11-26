using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace SpiderVR
{
    /// <summary>Cria o NetworkManager por codigo (nao depende de objeto na cena). O prefab do avatar e gerado no build em Resources/PlayerNet.</summary>
    public static class NetRuntime
    {
        public static NetworkManager Ensure()
        {
            if (NetworkManager.Singleton != null) return NetworkManager.Singleton;
            var go = new GameObject("NetworkManager");
            var ut = go.AddComponent<UnityTransport>();
            var nm = go.AddComponent<NetworkManager>();
            var prefab = Resources.Load<GameObject>("PlayerNet");
            nm.NetworkConfig = new NetworkConfig { NetworkTransport = ut, PlayerPrefab = prefab, EnableSceneManagement = true };
            if (prefab != null) nm.NetworkConfig.Prefabs.Add(new NetworkPrefab { Prefab = prefab });
            else Debug.LogWarning("SpiderVR: Resources/PlayerNet nao encontrado (multiplayer sem avatar).");
            return nm;
        }
    }
}
