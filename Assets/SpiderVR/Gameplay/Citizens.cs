using System.Collections.Generic;
using UnityEngine;

namespace SpiderVR
{
    /// <summary>Cidadao (pedestre da cidade): pode apanhar de bandidos, morrer e, se cair de um predio, ser salvo por uma teia.</summary>
    public class Citizen : MonoBehaviour, IDamageable
    {
        public static readonly List<Citizen> All = new List<Citizen>();
        public static int Saved; public static Citizen Falling;
        public float hp = 30f; public bool dead, webbed, fallingNow;
        Rigidbody rb;

        void OnEnable() { All.Add(this); rb = GetComponent<Rigidbody>(); }
        void OnDisable() { All.Remove(this); if (Falling == this) Falling = null; }

        public void Hit(float d, Vector3 dir) { if (dead) return; hp -= d; if (hp <= 0) Die(); }

        public void Die()
        {
            if (dead) return; dead = true; fallingNow = false; if (Falling == this) Falling = null;
            foreach (var m in GetComponents<MonoBehaviour>()) if (m.GetType().Name == "yayaHareket") m.enabled = false;
            var an = GetComponent<Animator>(); if (an) an.enabled = false;
            if (rb) { rb.constraints = RigidbodyConstraints.None; rb.AddTorque(transform.right * 3f, ForceMode.VelocityChange); }
            Destroy(gameObject, 25f);
        }

        public void StartFall(Vector3 push)
        {
            foreach (var m in GetComponents<MonoBehaviour>()) if (m.GetType().Name == "yayaHareket") m.enabled = false;
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = false; rb.constraints = RigidbodyConstraints.None; rb.velocity = push; rb.drag = 0f;
            fallingNow = true; Falling = this;
        }

        /// <summary>Teia no cidadao que cai: salva (amortece a queda).</summary>
        public void Webbed()
        {
            if (!fallingNow || webbed) return;
            webbed = true; Saved++;
            if (rb) { rb.drag = 3f; rb.velocity *= 0.3f; }
        }

        void OnCollisionEnter(Collision c)
        {
            if (!fallingNow) return;
            if (!webbed && c.relativeVelocity.magnitude > 12f) Die();
            else { fallingNow = false; if (rb) rb.drag = 0f; if (Falling == this) Falling = null; }
        }
    }

    /// <summary>De tempos em tempos um cidadao cai de um predio perto de voce. Solte a teia nele (mao direita) para salva-lo.</summary>
    public class CitizenFaller : MonoBehaviour
    {
        public Transform player; float next = 15f; GUIStyle st;

        void Update()
        {
            if (Time.time < next || player == null || Citizen.Falling != null) return;
            next = Time.time + Random.Range(25f, 45f);
            Citizen victim = null; foreach (var c in Citizen.All) if (c != null && !c.dead && !c.fallingNow) { victim = c; if (Random.value < 0.3f) break; }
            if (victim == null) return;
            for (int k = 0; k < 10; k++)   // acha o topo de um predio alto e a borda dele
            {
                Vector2 r = Random.insideUnitCircle.normalized * Random.Range(60f, 140f);
                var o = player.position + new Vector3(r.x, 800f, r.y);
                if (!Physics.Raycast(o, Vector3.down, out var h, 1600f, ~0, QueryTriggerInteraction.Ignore) || h.point.y < player.position.y + 10f) continue;
                Vector2 d2 = Random.insideUnitCircle.normalized; Vector3 dir = new Vector3(d2.x, 0, d2.y); Vector3 p = h.point;
                for (int i = 1; i <= 20; i++)
                {
                    Vector3 q = p + dir * (i * 1.5f);
                    if (!Physics.Raycast(q + Vector3.up * 5f, Vector3.down, out var h2, 1600f, ~0, QueryTriggerInteraction.Ignore) || h2.point.y < p.y - 10f)
                    {
                        victim.transform.position = p + dir * ((i - 0.3f) * 1.5f) + Vector3.up * 0.6f;
                        victim.StartFall(dir * 2f); return;
                    }
                }
            }
        }

        void OnGUI()
        {
            if (st == null) st = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = Screen.height / 28, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
            string t = "SALVOS: " + Citizen.Saved;
            if (Citizen.Falling != null && player != null) t += "\nCIDADAO CAINDO! " + (int)Vector3.Distance(player.position, Citizen.Falling.transform.position) + " m - solte a teia (mao direita)";
            foreach (float cx in new[] { Screen.width * 0.25f, Screen.width * 0.75f })
            { var r = new Rect(cx - Screen.width * 0.22f, Screen.height * 0.02f, Screen.width * 0.44f, Screen.height * 0.2f); GUI.color = Color.black; GUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), t, st); GUI.color = Citizen.Falling != null ? Color.yellow : Color.white; GUI.Label(r, t, st); }
        }
    }
}
