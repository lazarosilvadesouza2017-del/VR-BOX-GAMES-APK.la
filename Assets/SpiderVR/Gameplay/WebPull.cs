using UnityEngine;
namespace SpiderVR
{
    /// <summary>Marca carros/NPCs: ao ser puxado desliga o script de trafego e vira objeto de fisica.</summary>
    public class WebPullable : MonoBehaviour
    {
        public float mass = 80f;
        public void Release()
        {
            foreach (var m in GetComponents<MonoBehaviour>()) if (m != this && (m.GetType().Name == "hareketEt" || m.GetType().Name == "yayaHareket")) m.enabled = false;
            var rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = false; rb.mass = mass;
            if (GetComponentInChildren<Collider>() == null) gameObject.AddComponent<BoxCollider>();
        }
    }

    /// <summary>Mao direita (ou 2 dedos): teia que PUXA qualquer coisa (carro, bandido, pedestre).</summary>
    public class WebPull : MonoBehaviour
    {
        public GestureRecognizer gestures; public Transform head, wrist;
        public float maxDistance = 60f, force = 45f; public LayerMask mask = ~0;
        Rigidbody target; PlayerNet targetPlayer; LineRenderer lr; bool armed; float ticks;
        void Awake() { lr = new GameObject("PullLine").AddComponent<LineRenderer>(); lr.transform.SetParent(transform, false); lr.startWidth = lr.endWidth = 0.03f; lr.positionCount = 0; lr.material = new Material(Shader.Find("Sprites/Default")); lr.startColor = lr.endColor = Color.white; }
        void OnEnable() { if (gestures) { gestures.WebStart += S; gestures.WebEnd += E; } }
        void OnDisable() { if (gestures) { gestures.WebStart -= S; gestures.WebEnd -= E; } }
        void S(Side s)
        {
            if (s != Side.Right || !Physics.Raycast(head.position, head.forward, out var h, maxDistance, mask, QueryTriggerInteraction.Ignore)) return;
            var p = h.collider.GetComponentInParent<WebPullable>(); if (p) { p.Release(); target = p.GetComponent<Rigidbody>(); }
            else target = h.rigidbody;
            targetPlayer = h.collider.GetComponentInParent<PlayerNet>();
            if (target || targetPlayer) { lr.positionCount = 2; armed = GameSettings.Web == WebMode.Auto; if (!armed && gestures) gestures.Yank += Pulled; }
        }
        void Pulled(Side s) { armed = true; if (gestures) gestures.Yank -= Pulled; }
        void E(Side s) { if (s == Side.Right) { target = null; targetPlayer = null; armed = false; lr.positionCount = 0; if (gestures) gestures.Yank -= Pulled; } }
        void FixedUpdate()
        {
            if (targetPlayer && armed) { ticks += Time.fixedDeltaTime; if (ticks > 0.1f) { ticks = 0; targetPlayer.RequestImpulse((transform.position - targetPlayer.transform.position).normalized * 6f); } return; }
            if (!target || !armed) return;
            Vector3 dir = (transform.position + Vector3.up - target.worldCenterOfMass).normalized;
            target.AddForce(dir * force * Mathf.Min(target.mass, 300f));
        }
        void LateUpdate() { if ((target || targetPlayer) && lr.positionCount == 2) { lr.SetPosition(0, wrist ? wrist.position : transform.position); lr.SetPosition(1, target ? target.worldCenterOfMass : targetPlayer.transform.position); } }
    }
}
