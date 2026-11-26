using UnityEngine;
namespace SpiderVR
{
    public class WeaponPickup : MonoBehaviour { public int ammo = 24; public float damage = 18f; }

    /// <summary>Mao estendida perto da arma + gesto de arminha = pega. Com arma, segurar o gesto = atirar.</summary>
    public class GunHandler : MonoBehaviour
    {
        public GestureRecognizer gestures; public Transform head; public float pickRange = 4f, rate = 5f, range = 80f;
        bool gun; int ammo; float damage, next; LineRenderer tr; float trOff;
        void OnEnable() { if (gestures) { gestures.GunStart += s => gun = true; gestures.GunEnd += s => gun = false; } }
        void Awake() { tr = new GameObject("Tracer").AddComponent<LineRenderer>(); tr.transform.SetParent(transform, false); tr.startWidth = tr.endWidth = 0.02f; tr.material = new Material(Shader.Find("Sprites/Default")); tr.startColor = tr.endColor = Color.yellow; tr.positionCount = 0; }
        void Update()
        {
            if (tr.positionCount > 0 && Time.time > trOff) tr.positionCount = 0;
            if (!gun) return;
            if (ammo <= 0) { TryPick(); return; }
            if (Time.time < next) return; next = Time.time + 1f / rate; ammo--;
            Vector3 end = head.position + head.forward * range;
            if (Physics.Raycast(head.position, head.forward, out var h, range, ~0, QueryTriggerInteraction.Ignore))
            {
                end = h.point; h.collider.GetComponentInParent<IDamageable>()?.Hit(damage, head.forward);
                if (h.rigidbody) h.rigidbody.AddForceAtPosition(head.forward * 8f, h.point, ForceMode.Impulse);
            }
            tr.positionCount = 2; tr.SetPosition(0, head.position + head.right * 0.2f - head.up * 0.15f); tr.SetPosition(1, end); trOff = Time.time + 0.05f;
        }
        void TryPick()
        {
            foreach (var c in Physics.OverlapSphere(head.position + head.forward * 1.5f, pickRange, ~0, QueryTriggerInteraction.Collide))
            { var w = c.GetComponentInParent<WeaponPickup>(); if (w) { ammo = w.ammo; damage = w.damage; Destroy(w.gameObject); return; } }
        }
    }
}
