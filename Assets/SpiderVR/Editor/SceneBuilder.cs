#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpiderVR.Editor
{
    /// <summary>Monta a cena final: cidade+carros+pedestres do Traffic COM o jogador/teia do Swinging Simulator.</summary>
    public static class SceneBuilder
    {
        public const float CityScale = 2.5f;   // predios maiores = mais altura para balancar
        public const string Out = "Assets/Scenes/SpiderVR_Game.unity";
        static readonly string[] playerRoots = { "Player", "Camera Holder", "Orientation", "hand position", "First Person", "Swinging Music" };

        [MenuItem("SpiderVR/Montar cena final")]
        public static void Build()
        {
            var main = EditorSceneManager.OpenScene("Assets/Scenes/Main Game.unity", OpenSceneMode.Single);
            var move = new List<GameObject>();
            foreach (var go in main.GetRootGameObjects())
                if (System.Array.IndexOf(playerRoots, go.name) >= 0 || go.GetComponentInChildren<Canvas>(true) != null || go.GetComponent<UnityEngine.EventSystems.EventSystem>() != null)
                    move.Add(go);

            var city = EditorSceneManager.OpenScene("Assets/Traffic/Scenes/trafikVeYayalar.unity", OpenSceneMode.Additive);
            foreach (var go in move) SceneManager.MoveGameObjectToScene(go, city);
            SceneManager.SetActiveScene(city);
            EditorSceneManager.CloseScene(main, true);

            foreach (var go in city.GetRootGameObjects())
            {
                if (System.Array.IndexOf(playerRoots, go.name) >= 0 || go.GetComponentInChildren<Canvas>(true) != null) continue;
                if (go.GetComponent<Light>() || go.GetComponent<Camera>() || go.GetComponent<UnityEngine.EventSystems.EventSystem>()) continue;
                go.transform.position *= CityScale; go.transform.localScale *= CityScale;
            }
            foreach (var c in Object.FindObjectsOfType<hareketEt>()) c.transform.localScale /= CityScale;   // carros e pedestres voltam ao tamanho normal
            foreach (var p in Object.FindObjectsOfType<yayaHareket>()) p.transform.localScale /= CityScale;
            int wall = LayerMask.NameToLayer("whatIsWall"), ground = LayerMask.NameToLayer("whatIsGround");
            var groundRx = new Regex("road|street|ground|grass|plane|asphalt|sidewalk|yol|zemin", RegexOptions.IgnoreCase);
            Bounds b = new Bounds(Vector3.zero, Vector3.zero); bool first = true; int cars = 0, peds = 0;
            Transform player = null; foreach (var go in city.GetRootGameObjects()) if (go.name == "Player") player = go.transform;

            foreach (var mr in Object.FindObjectsOfType<MeshRenderer>())
            {
                if (player && mr.transform.IsChildOf(player)) continue;
                var t = mr.transform;
                bool vehicle = t.GetComponentInParent<hareketEt>() != null, ped = t.GetComponentInParent<yayaHareket>() != null;
                if (vehicle || ped) continue;
                if (mr.GetComponent<Collider>() == null && mr.GetComponent<MeshFilter>() && mr.GetComponent<MeshFilter>().sharedMesh) mr.gameObject.AddComponent<MeshCollider>();
                mr.gameObject.layer = groundRx.IsMatch(t.name) ? ground : wall;
                if (first) { b = mr.bounds; first = false; } else b.Encapsulate(mr.bounds);
            }
            foreach (var c in Object.FindObjectsOfType<hareketEt>()) { if (!c.GetComponent<WebPullable>()) { c.gameObject.AddComponent<WebPullable>().mass = 1200; cars++; } }
            foreach (var p in Object.FindObjectsOfType<yayaHareket>()) { if (!p.GetComponent<WebPullable>()) { p.gameObject.AddComponent<WebPullable>().mass = 80; peds++; } }

            if (player) player.position = new Vector3(b.center.x, b.max.y + 3f, b.center.z);
            EditorSceneManager.SaveScene(city, Out);
            Debug.Log("SpiderVR: cena montada. carros=" + cars + " pedestres=" + peds + " bounds=" + b);

            var scenes = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene("Assets/Scenes/Menu.unity", true), new EditorBuildSettingsScene(Out, true) };
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
