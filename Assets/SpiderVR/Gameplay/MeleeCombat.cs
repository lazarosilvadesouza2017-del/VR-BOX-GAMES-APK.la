using UnityEngine;

namespace SpiderVR
{
    public interface IDamageable { void Hit(float dmg, Vector3 dir); }

    /// <summary>Soco e chute reais (gesto) viram ataque no jogo: esfera na frente da cabeca + impulso.</summary>
    public class MeleeCombat : MonoBehaviour
    {
        public GestureRecognizer gestures; public Transform head;
        public float punchRange = 1.8f, kickRange = 2.2f, radius = 0.8f, punchDmg = 15, kickDmg = 25, impulse = 6f;
        public LayerMask mask = ~0;
        void OnEnable() { if (gestures) { gestures.Punch += s => Strike(punchRange, punchDmg); gestures.Kick += s => Strike(kickRange, kickDmg); } }

        void Strike(float range, float dmg)
        {
            Vector3 c = head.position + head.forward * range * 0.6f;
            foreach (var col in Physics.OverlapSphere(c, radius, mask, QueryTriggerInteraction.Ignore))
            {
                if (col.transform.root == transform.root) continue;
                Vector3 dir = (col.bounds.center - head.position).normalized;
                if (col.attachedRigidbody) col.attachedRigidbody.AddForce(dir * impulse * Mathf.Min(col.attachedRigidbody.mass, 100f), ForceMode.Impulse);
                col.GetComponentInParent<IDamageable>()?.Hit(dmg, dir);
            }
        }
    }
}
