#if UNITY_EDITOR
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SpiderVR.Editor
{
    /// <summary>Gera Resources/PlayerNet.prefab (avatar em rede). Roda sozinho antes de TODO build (inclusive Unity Cloud Build).</summary>
    public class NetSetup : IPreprocessBuildWithReport
    {
        const string PrefabPath = "Assets/SpiderVR/Resources/PlayerNet.prefab";
        public int callbackOrder => -1000;
        public void OnPreprocessBuild(BuildReport report) { CreatePrefab(); }

        [MenuItem("SpiderVR/Configurar rede")]
        public static void Run() { CreatePrefab(); }

        public static void CreatePrefab()
        {
            System.IO.Directory.CreateDirectory("Assets/SpiderVR/Resources");
            var root = new GameObject("PlayerNet");
            var cap = root.AddComponent<CapsuleCollider>(); cap.height = 1.8f; cap.radius = 0.35f; cap.center = Vector3.up * 0.9f;
            root.AddComponent<NetworkObject>(); root.AddComponent<ClientNetworkTransform>(); root.AddComponent<PlayerNet>();
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath); Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        }
    }
}
#endif
