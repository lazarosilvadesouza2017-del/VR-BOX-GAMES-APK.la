using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpiderVR
{
    /// <summary>
    /// Monta a cidade final EM TEMPO DE EXECUCAO (nao depende de passo manual no editor / Cloud Build):
    /// carrega a cidade do Traffic por cima da cena "Main Game" (jogador + teia), desliga a cidade antiga,
    /// escala os predios, ajusta camadas/colisores e coloca o jogador no alto.
    /// </summary>
    public static class CityComposer
    {
        public const float CityScale = 2.5f;
        const string CityScene = "trafikVeYayalar";
        static readonly string[] keep = { "Player", "Camera Holder", "Orientation", "hand position", "First Person", "Swinging Music" };

        public static IEnumerator Compose(PlayerMovement pm)
        {
            if (!Application.CanStreamedLevelBeLoaded(CityScene)) yield break;   // sem a cidade nova: fica a cidade original
            var main = pm.gameObject.scene;
            var op = SceneManager.LoadSceneAsync(CityScene, LoadSceneMode.Additive); yield return op;
            var city = SceneManager.GetSceneByName(CityScene); if (!city.IsValid()) yield break;

            foreach (var go in main.GetRootGameObjects())   // desliga a cidade antiga do Swinging Simulator
                if (System.Array.IndexOf(keep, go.name) < 0 && go.GetComponentInChildren<Canvas>(true) == null && go.GetComponentInChildren<Camera>(true) == null
                    && go.GetComponent<Light>() == null && go.GetComponent<UnityEngine.EventSystems.EventSystem>() == null && go.GetComponentInChildren<AudioSource>(true) == null)
                    go.SetActive(false);

            foreach (var go in city.GetRootGameObjects())
            {
                foreach (var c in go.GetComponentsInChildren<Camera>(true)) c.enabled = false;
                foreach (var a in go.GetComponentsInChildren<AudioListener>(true)) a.enabled = false;
                foreach (var cs in go.GetComponentsInChildren<cameraSwitch>(true)) cs.enabled = false;
                foreach (var l in go.GetComponentsInChildren<Light>(true)) if (l.type == LightType.Directional) l.enabled = false;
                go.transform.position *= CityScale; go.transform.localScale *= CityScale;
            }
            foreach (var c in Object.FindObjectsOfType<hareketEt>()) c.transform.localScale /= CityScale;   // carros e pedestres no tamanho normal
            foreach (var p in Object.FindObjectsOfType<yayaHareket>()) p.transform.localScale /= CityScale;

            int wall = LayerMask.NameToLayer("whatIsWall"), ground = LayerMask.NameToLayer("whatIsGround");
            var groundRx = new Regex("road|street|ground|grass|plane|asphalt|sidewalk|yol|zemin", RegexOptions.IgnoreCase);
            Bounds b = new Bounds(pm.transform.position, Vector3.zero); bool first = true; int n = 0;
            foreach (var go in city.GetRootGameObjects())
                foreach (var mr in go.GetComponentsInChildren<MeshRenderer>(true))
                {
                    var t = mr.transform;
                    if (t.GetComponentInParent<hareketEt>() != null || t.GetComponentInParent<yayaHareket>() != null) continue;
                    var mf = mr.GetComponent<MeshFilter>();
                    if (mr.GetComponent<Collider>() == null && mf != null && mf.sharedMesh != null && mf.sharedMesh.isReadable) mr.gameObject.AddComponent<MeshCollider>();
                    if (wall >= 0 && ground >= 0) mr.gameObject.layer = groundRx.IsMatch(t.name) ? ground : wall;
                    if (first) { b = mr.bounds; first = false; } else b.Encapsulate(mr.bounds);
                    if (++n % 40 == 0) yield return null;   // nao trava o celular
                }
            foreach (var c in Object.FindObjectsOfType<hareketEt>()) if (!c.GetComponent<WebPullable>()) c.gameObject.AddComponent<WebPullable>().mass = 1200;
            foreach (var p in Object.FindObjectsOfType<yayaHareket>()) if (!p.GetComponent<WebPullable>()) p.gameObject.AddComponent<WebPullable>().mass = 80;

            if (!first) pm.transform.position = new Vector3(b.center.x, b.max.y + 3f, b.center.z);
            var rb = pm.GetComponent<Rigidbody>(); if (rb) rb.velocity = Vector3.zero;
            Physics.SyncTransforms();
        }
    }
}
