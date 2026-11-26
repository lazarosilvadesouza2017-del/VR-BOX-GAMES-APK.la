using UnityEngine;
namespace SpiderVR
{
    /// <summary>
    /// Ao entrar no mapa (solo ou multiplayer): escolher o traje. Cada traje mostra nome e "foto" (retrato 3D renderizado uma vez).
    /// O jogador fica parado enquanto escolhe. A escolha fica salva e e enviada aos outros jogadores.
    /// </summary>
    public class SuitSelectUI : MonoBehaviour
    {
        public PlayerMovement pm;
        Texture2D[] photos; int bakeIdx, sel; bool open = true; Rigidbody rb; bool wasKinematic; GUIStyle nameStyle, title;

        void Start()
        {
            photos = new Texture2D[SuitCatalog.All.Length];
            for (int i = 0; i < SuitCatalog.All.Length; i++) if (SuitCatalog.All[i].id == GameSettings.SuitId) sel = i;
            if (pm) { rb = pm.GetComponent<Rigidbody>(); if (rb) { wasKinematic = rb.isKinematic; rb.isKinematic = true; } }
        }

        void Update() { if (open && bakeIdx < photos.Length) Bake(bakeIdx++); }   // 1 foto por frame: sem travada

        void Bake(int i)
        {
            var info = SuitCatalog.All[i];
            var holder = new GameObject("bake"); holder.transform.position = new Vector3(0, -5000, 0);
            var go = SuitCatalog.Create(info.id, holder.transform);
            if (go == null) { Destroy(holder); return; }
            foreach (var t in go.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = 30;
            var camGo = new GameObject("bakecam"); var c = camGo.AddComponent<Camera>(); c.enabled = false;
            c.clearFlags = CameraClearFlags.SolidColor; c.backgroundColor = new Color(0.1f, 0.12f, 0.22f); c.cullingMask = 1 << 30;
            c.orthographic = true; c.orthographicSize = 1.0f; c.nearClipPlane = 0.1f; c.farClipPlane = 20f;
            camGo.transform.position = holder.transform.position + new Vector3(0, 0.95f, 4f); camGo.transform.LookAt(holder.transform.position + Vector3.up * 0.95f);
            var l = new GameObject("bakelight").AddComponent<Light>(); l.type = LightType.Directional; l.cullingMask = 1 << 30; l.intensity = 1.2f; l.transform.rotation = Quaternion.Euler(25, 180 + 20, 0);
            var rt = RenderTexture.GetTemporary(192, 256, 16); c.targetTexture = rt; c.Render();
            var prev = RenderTexture.active; RenderTexture.active = rt;
            var tex = new Texture2D(192, 256, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, 192, 256), 0, 0); tex.Apply();
            RenderTexture.active = prev; c.targetTexture = null; RenderTexture.ReleaseTemporary(rt);
            photos[i] = tex; Destroy(holder); Destroy(camGo); Destroy(l.gameObject);
        }

        void OnGUI()
        {
            if (!open) return;
            float s = Screen.height / 540f; GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1)); float w = Screen.width / s;
            if (title == null) { title = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 26, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } }; nameStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = { textColor = Color.white } }; }
            GUI.color = new Color(0, 0, 0, 0.9f); GUI.DrawTexture(new Rect(0, 0, w, 540), Texture2D.whiteTexture); GUI.color = Color.white;
            GUI.Label(new Rect(0, 6, w, 36), "ESCOLHA SEU TRAJE", title);
            const int cols = 5; float cw = (w - 20) / cols, ch = 140;
            for (int i = 0; i < SuitCatalog.All.Length; i++)
            {
                var r = new Rect(10 + (i % cols) * cw, 46 + (i / cols) * (ch + 6), cw - 6, ch);
                GUI.color = i == sel ? new Color(0.9f, 0.2f, 0.2f) : new Color(0.25f, 0.25f, 0.3f); GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = Color.white;
                if (photos[i]) GUI.DrawTexture(new Rect(r.x + (r.width - ch * 0.62f) / 2, r.y + 2, ch * 0.62f, ch * 0.82f), photos[i], ScaleMode.ScaleToFit);
                GUI.Label(new Rect(r.x, r.y + ch - 40, r.width, 40), SuitCatalog.All[i].name, nameStyle);
                if (GUI.Button(r, GUIContent.none, GUIStyle.none)) sel = i;
            }
            if (GUI.Button(new Rect(w / 2 - 150, 490, 300, 40), "JOGAR COM " + SuitCatalog.All[sel].name.ToUpper())) Confirm();
        }

        void Confirm()
        {
            GameSettings.SuitId = SuitCatalog.All[sel].id;
            foreach (var p in FindObjectsOfType<PlayerNet>()) if (p.IsOwner) p.SetSuit(GameSettings.SuitId);
            if (rb) rb.isKinematic = wasKinematic;
            open = false; foreach (var t in photos) if (t) Destroy(t);
        }
    }
}
