using System.Collections.Generic;
using UnityEngine;

namespace SpiderVR
{
    /// <summary>
    /// 1) Desliga renderers de predios distantes (amortizado: poucos por frame).
    /// 2) SkylineBackdrop: cilindro com PNG de horizonte que segue o jogador e cobre o fundo.
    /// 3) ScaleCity: multiplica o tamanho da cidade (x2/x3) para a teia ter mais altura.
    /// </summary>
    public class CityOptimizer : MonoBehaviour
    {
        public Transform player; public Transform cityRoot;
        public float renderDistance = 220f; public int perFrame = 40;
        public Texture2D skylinePng; public float backdropRadius = 500f, backdropHeight = 260f;
        public float cityScale = 1f;
        List<Renderer> rs = new List<Renderer>(); int cursor; Transform backdrop;

        void Start()
        {
            if (cityRoot == null) foreach (var r in FindObjectsOfType<Renderer>()) { if (player && r.transform.IsChildOf(player)) continue; if (r is LineRenderer || r is ParticleSystemRenderer) continue; rs.Add(r); }
            else { cityRoot.localScale *= cityScale; foreach (var r in cityRoot.GetComponentsInChildren<Renderer>(true)) rs.Add(r); }
            if (skylinePng) BuildBackdrop();
        }

        void Update()
        {
            if (!player) return;
            float sq = renderDistance * renderDistance;
            for (int i = 0; i < perFrame && rs.Count > 0; i++)
            {
                cursor = (cursor + 1) % rs.Count; var r = rs[cursor]; if (!r) continue;
                bool on = (r.bounds.center - player.position).sqrMagnitude < sq;
                if (r.enabled != on) r.enabled = on;
            }
            if (backdrop) backdrop.position = new Vector3(player.position.x, 0, player.position.z);
        }

        void BuildBackdrop()
        {
            var go = new GameObject("SkylineBackdrop"); backdrop = go.transform;
            const int seg = 24; var v = new Vector3[(seg + 1) * 2]; var uv = new Vector2[v.Length]; var t = new int[seg * 6];
            for (int i = 0; i <= seg; i++)
            {
                float a = i / (float)seg * Mathf.PI * 2f; float x = Mathf.Cos(a) * backdropRadius, z = Mathf.Sin(a) * backdropRadius;
                v[i * 2] = new Vector3(x, 0, z); v[i * 2 + 1] = new Vector3(x, backdropHeight, z);
                uv[i * 2] = new Vector2(i / (float)seg * 2f, 0); uv[i * 2 + 1] = new Vector2(i / (float)seg * 2f, 1);
            }
            for (int i = 0; i < seg; i++) { int b = i * 2, k = i * 6; t[k] = b; t[k + 1] = b + 2; t[k + 2] = b + 1; t[k + 3] = b + 1; t[k + 4] = b + 2; t[k + 5] = b + 3; }
            var m = new Mesh { vertices = v, uv = uv, triangles = t }; m.RecalculateBounds();
            go.AddComponent<MeshFilter>().sharedMesh = m;
            var mat = new Material(Shader.Find("Sprites/Default")) { mainTexture = skylinePng };
            var mr = go.AddComponent<MeshRenderer>(); mr.sharedMaterial = mat; mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            skylinePng.wrapMode = TextureWrapMode.Repeat;
            // desenhar no fundo: depois do ceu, antes do resto fica por conta do raio > far dos predios renderizados
        }
    }
}
