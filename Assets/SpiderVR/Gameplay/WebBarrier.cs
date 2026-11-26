using UnityEngine;
namespace SpiderVR
{
    public class WebWall : MonoBehaviour, IDamageable
    {
        public float hp = 30f, life = 25f;
        void Start() { Destroy(gameObject, life); }
        public void Hit(float dmg, Vector3 dir) { hp -= dmg; if (hp <= 0) Destroy(gameObject); }
    }

    /// <summary>Teia presa ao chao/predio: o objeto/NPC/jogador fica no limite da corda. Puxao (Yank) o joga no chao.</summary>
    public class WebTether : MonoBehaviour
    {
        public Vector3 point; public float length = 6f; Rigidbody rb; LineRenderer lr; float expire;
        public static WebTether Attach(Rigidbody target, Vector3 anchor, float len)
        {
            var t = target.gameObject.GetComponent<WebTether>() ?? target.gameObject.AddComponent<WebTether>();
            t.rb = target; t.point = anchor; t.length = len; t.expire = Time.time + 20f;
            if (t.lr == null) { t.lr = new GameObject("Tether").AddComponent<LineRenderer>(); t.lr.transform.SetParent(t.transform, false); t.lr.startWidth = t.lr.endWidth = 0.04f; t.lr.positionCount = 2; t.lr.material = new Material(Shader.Find("Sprites/Default")); }
            return t;
        }
        void FixedUpdate()
        {
            if (rb == null || Time.time > expire) { Destroy(this); return; }
            Vector3 d = rb.position - point;
            if (d.magnitude > length)
            {
                rb.position = point + d.normalized * length;
                rb.velocity = Vector3.ProjectOnPlane(rb.velocity, d.normalized) + Mathf.Min(0, Vector3.Dot(rb.velocity, d.normalized)) * d.normalized * 0f;
            }
        }
        void LateUpdate() { if (lr) { lr.SetPosition(0, point); lr.SetPosition(1, transform.position); } }
        public void Yank() { if (rb) { rb.velocity = Vector3.zero; rb.AddForce(Vector3.down * 40f, ForceMode.VelocityChange); rb.AddForce((point - rb.position).normalized * 15f, ForceMode.VelocityChange); } Destroy(this); }
        void OnDestroy() { if (lr) Destroy(lr.gameObject); }
    }

    /// <summary>Arremesso: carro/NPC lancado em direcao a outro alvo, leva o alvo junto e solta (fling).</summary>
    public class WebCarrier : MonoBehaviour
    {
        public Rigidbody a, b; public PlayerNet bPlayer; float t0;
        public static void Launch(Rigidbody a, Rigidbody b, PlayerNet bp)
        {
            var c = a.gameObject.AddComponent<WebCarrier>(); c.a = a; c.b = b; c.bPlayer = bp; c.t0 = Time.time;
            foreach (var m in a.GetComponents<MonoBehaviour>()) if (m.GetType().Name == "hareketEt") m.enabled = false;
        }
        void FixedUpdate()
        {
            Vector3 tp = b ? b.position : bPlayer.transform.position; float el = Time.time - t0;
            if (el < 1.2f) { a.velocity = (tp - a.position).normalized * 45f; }                   // hiper veloz ate o alvo
            else if (el < 3.2f)                                                                    // leva o alvo junto
            {
                Vector3 v = a.velocity.sqrMagnitude > 4 ? a.velocity : transform.forward * 30f;
                if (b) { b.position = a.position + Vector3.up * 1.5f; b.velocity = v; } else if (bPlayer) bPlayer.RequestImpulse((a.position - bPlayer.transform.position) * 2f + v * 0.2f);
            }
            else
            {
                Vector3 dir = (Vector3.up * 0.6f + a.velocity.normalized).normalized * 40f;         // fling
                if (b) b.velocity = dir; else if (bPlayer) bPlayer.RequestImpulse(dir);
                Destroy(this);
            }
        }
    }

    /// <summary>
    /// Duas teias + JUNTAR as maos:
    ///  - ambas em coisas soltas (carro/NPC/jogador): A voa ate B, leva e arremessa;
    ///  - uma em coisa solta e outra no chao/predio: prende (corda) - puxao (Yank) joga no chao;
    ///  - ambas em estrutura: barreira de teia (so some com soco/chute).
    /// </summary>
    public class WebBarrier : MonoBehaviour
    {
        public static event System.Action Joined;
        public GestureRecognizer gestures; public Transform head;
        public float maxDistance = 60f, joinDist = 0.12f;
        Vector3[] anchor = new Vector3[2]; bool[] set = new bool[2]; Rigidbody[] rbs = new Rigidbody[2]; PlayerNet[] pl = new PlayerNet[2]; Material mat;

        void OnEnable() { if (gestures) { gestures.WebStart += S; gestures.Yank += Y; } }
        void OnDisable() { if (gestures) { gestures.WebStart -= S; gestures.Yank -= Y; } }

        void S(Side s)
        {
            if (!Physics.Raycast(head.position, head.forward, out var h, maxDistance, ~0, QueryTriggerInteraction.Ignore)) return;
            int i = (int)s; anchor[i] = h.point; set[i] = true;
            var p = h.collider.GetComponentInParent<WebPullable>(); if (p) p.Release();
            rbs[i] = h.rigidbody; pl[i] = h.collider.GetComponentInParent<PlayerNet>();
            if (rbs[i] && rbs[i].transform.root == transform.root) rbs[i] = null;
        }

        void Y(Side s)
        {
            foreach (var t in FindObjectsOfType<WebTether>()) t.Yank();
        }

        void Update()
        {
            if (!set[0] || !set[1]) return;
            bool join = false;
            if (gestures.source != null && gestures.source.TryGet(out var f) && f.LeftWrist.valid && f.RightWrist.valid) join = Vector2.Distance(f.LeftWrist.p, f.RightWrist.p) < joinDist;
            else join = Input.touchCount >= 3;
            if (join) Resolve();
        }

        void Resolve()
        {
            set[0] = set[1] = false; Joined?.Invoke();
            bool l0 = rbs[0] || pl[0], l1 = rbs[1] || pl[1];
            if (l0 && l1) { var a = rbs[0] ? rbs[0] : rbs[1]; var other = a == rbs[0] ? 1 : 0; if (a != null) WebCarrier.Launch(a, rbs[other], pl[other]); }
            else if (l0 || l1)
            {
                int o = l0 ? 0 : 1, f = 1 - o;
                if (rbs[o]) WebTether.Attach(rbs[o], anchor[f], Mathf.Max(3f, Vector3.Distance(anchor[o], anchor[f]) * 0.8f));
            }
            else Wall(anchor[0], anchor[1]);
        }

        void Wall(Vector3 a, Vector3 b)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = "WebWall";
            float len = Vector3.Distance(a, b), hgt = Mathf.Clamp(len * 0.6f, 2f, 8f);
            go.transform.position = (a + b) * 0.5f + Vector3.up * hgt * 0.5f;
            Vector3 fwd = Vector3.Cross(b - a, Vector3.up); if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
            go.transform.rotation = Quaternion.LookRotation(fwd.normalized, Vector3.up);
            go.transform.localScale = new Vector3(len, hgt, 0.08f);
            if (mat == null) { mat = new Material(Shader.Find("Sprites/Default")); mat.color = new Color(1, 1, 1, 0.35f); }
            go.GetComponent<Renderer>().sharedMaterial = mat; go.AddComponent<WebWall>();
        }
    }
}
