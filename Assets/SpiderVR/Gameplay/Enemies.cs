using UnityEngine;
namespace SpiderVR
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        public float hp = 100; float regen;
        public void Hit(float d, Vector3 dir) { hp -= d; regen = Time.time + 6f; if (hp <= 0) { hp = 100; transform.position += Vector3.up * 30f; } }
        void Update() { if (Time.time > regen && hp < 100) hp = Mathf.Min(100, hp + 8 * Time.deltaTime); }
        void OnGUI() { GUI.color = Color.red; GUI.Label(new Rect(10, 10, 200, 24), "HP " + (int)hp); }
    }

    /// <summary>Bandido simples: persegue e bate. Ao ser derrotado larga uma arma. Pode ser puxado pela teia.</summary>
    public class Bandit : MonoBehaviour, IDamageable
    {
        public float hp = 60, speed = 3.5f, sight = 35f, dmg = 8f; Transform target; Rigidbody rb; float atk;
        void Start() { rb = GetComponent<Rigidbody>(); var p = FindObjectOfType<PlayerMovement>(); if (p) target = p.transform; }
        Citizen victim; float scan;
        void FixedUpdate()
        {
            if (hp <= 0) return;
            if (Time.time > scan)   // escolhe a vitima: cidadao mais proximo (ate 30 m); senao o jogador
            {
                scan = Time.time + 0.5f; victim = null; float best = 30f * 30f;
                foreach (var c in Citizen.All) { if (c == null || c.dead || c.fallingNow) continue; float dd = (c.transform.position - transform.position).sqrMagnitude; if (dd < best) { best = dd; victim = c; } }
            }
            Transform tgt = victim ? victim.transform : target; if (!tgt) return;
            Vector3 d = tgt.position - transform.position; d.y = 0; float m = d.magnitude;
            if (!victim && m > sight) return;
            if (m < 1.8f)
            {
                if (Time.time > atk) { atk = Time.time + 1.2f; IDamageable v = victim ? (IDamageable)victim : target.GetComponent<IDamageable>(); if (v != null) v.Hit(dmg, d.normalized); }
                return;
            }
            rb.MovePosition(rb.position + d.normalized * speed * Time.fixedDeltaTime); transform.rotation = Quaternion.LookRotation(d.normalized);
        }
        public void Hit(float d, Vector3 dir)
        {
            hp -= d; if (hp > 0) return;
            var w = GameObject.CreatePrimitive(PrimitiveType.Cube); w.name = "Arma"; w.transform.position = transform.position + Vector3.up; w.transform.localScale = new Vector3(0.15f, 0.15f, 0.6f);
            w.GetComponent<Renderer>().material.color = Color.black; w.AddComponent<Rigidbody>(); w.AddComponent<WeaponPickup>();
            Destroy(gameObject);
        }
    }

    /// <summary>Transforma pedestres da cidade em bandidos (NPCs reais do mapa) e monta bunkers com bandidos. Sem pedestres, usa capsulas.</summary>
    public class BanditSpawner : MonoBehaviour
    {
        public Transform player; public int count = 10, bunkers = 3; public float radius = 150f;
        void Start()
        {
            int n = 0;
            foreach (var p in FindObjectsOfType<yayaHareket>())
            {
                if (n >= count) break; if (Random.value > 0.35f) continue;
                if ((p.transform.position - player.position).sqrMagnitude > radius * radius * 4) continue;
                p.enabled = false; MakeBandit(p.gameObject); n++;
            }
            foreach (var p in FindObjectsOfType<yayaHareket>()) if (p.GetComponent<Bandit>() == null && p.GetComponent<Citizen>() == null) p.gameObject.AddComponent<Citizen>();
            for (; n < count; n++) { var g = Capsule(RandomGround()); if (g != null) MakeBandit(g); }
            for (int i = 0; i < bunkers; i++) Bunker(RandomGround());
        }
        Vector3? RandomGround()
        {
            for (int k = 0; k < 8; k++)
            {
                Vector2 r = Random.insideUnitCircle * radius; var o = player.position + new Vector3(r.x, 300f, r.y);
                if (Physics.Raycast(o, Vector3.down, out var h, 600f, ~0, QueryTriggerInteraction.Ignore) && h.normal.y > 0.9f) return h.point;
            }
            return null;
        }
        GameObject Capsule(Vector3? at)
        {
            if (at == null) return null;
            var b = GameObject.CreatePrimitive(PrimitiveType.Capsule); b.transform.position = at.Value + Vector3.up; b.GetComponent<Renderer>().material.color = new Color(0.25f, 0.1f, 0.1f); return b;
        }
        void MakeBandit(GameObject g)
        {
            g.name = "Bandido"; var rb = g.GetComponent<Rigidbody>() ?? g.AddComponent<Rigidbody>(); rb.mass = 80; rb.constraints = RigidbodyConstraints.FreezeRotation;
            if (g.GetComponentInChildren<Collider>() == null) { var c = g.AddComponent<CapsuleCollider>(); c.height = 1.8f; c.center = Vector3.up * 0.9f; c.radius = 0.35f; }
            if (!g.GetComponent<Bandit>()) g.AddComponent<Bandit>(); if (!g.GetComponent<WebPullable>()) g.AddComponent<WebPullable>();
        }
        void Bunker(Vector3? at)
        {
            if (at == null) return; var c = at.Value; var root = new GameObject("Bunker").transform; root.position = c;
            for (int i = 0; i < 4; i++)
            {
                var w = GameObject.CreatePrimitive(PrimitiveType.Cube); w.transform.SetParent(root, false);
                bool ns = i < 2; w.transform.localPosition = new Vector3(ns ? 0 : (i == 2 ? -6 : 6), 2, ns ? (i == 0 ? -6 : 6) : 0);
                w.transform.localScale = ns ? new Vector3(12, 4, 0.5f) : new Vector3(0.5f, 4, 12); w.GetComponent<Renderer>().material.color = Color.gray;
            }
            var door = root.GetChild(0); door.localScale = new Vector3(5, 4, 0.5f); door.localPosition = new Vector3(-3.5f, 2, -6);
            for (int i = 0; i < 4; i++) { var g = Capsule(c + new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f))); if (g != null) { MakeBandit(g); g.transform.SetParent(null); } }
        }
    }
}
