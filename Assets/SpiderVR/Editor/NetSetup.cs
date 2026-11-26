#if UNITY_EDITOR
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SpiderVR.Editor
{
    /// <summary>Cria o prefab de avatar em rede (corpo com o traje .glb) e o NetworkManager na cena Menu.</summary>
    public static class NetSetup
    {
        const string PrefabPath = "Assets/SpiderVR/Resources/PlayerNet.prefab";

        [MenuItem("SpiderVR/Configurar rede")]
        public static void Run()
        {
            System.IO.Directory.CreateDirectory("Assets/SpiderVR/Resources");
            var root = new GameObject("PlayerNet");
            var cap = root.AddComponent<CapsuleCollider>(); cap.height = 1.8f; cap.radius = 0.35f; cap.center = Vector3.up * 0.9f;
            root.AddComponent<NetworkObject>(); root.AddComponent<ClientNetworkTransform>(); root.AddComponent<PlayerNet>();
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath); Object.DestroyImmediate(root);

            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity", OpenSceneMode.Single);
            foreach (var n in Object.FindObjectsOfType<NetworkManager>()) Object.DestroyImmediate(n.gameObject);
            var go = new GameObject("NetworkManager"); var nm = go.AddComponent<NetworkManager>(); var ut = go.AddComponent<UnityTransport>();
            nm.NetworkConfig = new NetworkConfig { NetworkTransport = ut, PlayerPrefab = prefab, EnableSceneManagement = true };
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets(); Debug.Log("SpiderVR: rede configurada.");
        }
    }
}
#endif
