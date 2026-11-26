using UnityEngine;
namespace SpiderVR
{
    /// <summary>Animacao procedural leve (os modelos vieram sem clipes): bracos e pernas balancam conforme a velocidade. Eixo = direita do avatar no mundo.</summary>
    public class SuitAnimator : MonoBehaviour
    {
        Transform[] limb = new Transform[4]; Quaternion[] rest = new Quaternion[4]; Vector3 last; float speed, phase; Transform body;
        static readonly string[] key = { "LArm1", "RArm1", "LLeg1", "RLeg1" };
        static readonly float[] sign = { 1, -1, -1, 1 };

        void Start()
        {
            body = transform.parent ? transform.parent : transform; last = body.position;
            foreach (var t in GetComponentsInChildren<Transform>(true))
                for (int i = 0; i < 4; i++) if (limb[i] == null && t.name.EndsWith(key[i])) { limb[i] = t; rest[i] = t.localRotation; }
        }

        void LateUpdate()
        {
            float dt = Mathf.Max(Time.deltaTime, 0.001f); float v = (body.position - last).magnitude / dt; last = body.position;
            speed = Mathf.Lerp(speed, Mathf.Min(v, 12f), 0.15f); phase += dt * (3f + speed * 0.7f);
            float amp = Mathf.Clamp01(speed / 6f) * 45f;
            for (int i = 0; i < 4; i++)
            {
                if (limb[i] == null || limb[i].parent == null) continue;
                Vector3 axis = Quaternion.Inverse(limb[i].parent.rotation) * body.right;
                limb[i].localRotation = Quaternion.AngleAxis(Mathf.Sin(phase) * amp * sign[i], axis) * rest[i];
            }
        }
    }
}
