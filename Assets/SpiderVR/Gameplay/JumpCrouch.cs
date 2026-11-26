using UnityEngine;
namespace SpiderVR
{
    /// <summary>
    /// Pulo e agachar. Camera (pose): agachar = joelho dobrado/quadril descendo; pulo = quadril sobe rapido.
    /// Sem pose: acelerometro (impulso para cima = pulo) e a cabeca descendo (queda do eixo Y filtrada) = agachar.
    /// Forca: pulinho (fraco) / normal / alto. Se estava correndo => pulo com impulso para frente.
    /// </summary>
    public class JumpCrouch : MonoBehaviour
    {
        public PlayerMovement pm; public RunLocomotion run; public GestureRecognizer gestures; public Transform head;
        public static event System.Action Jumped, Crouched;
        float crouchUntil, jumpAt = -10; bool wasCrouch;
        public float crouchDrop = 0.06f, jumpRise = 0.9f;
        float baseY = -1, lastY, lastT; float crouchAmt; float runMemory; float accBase = 1f; float cd;
        

        Vector3 baseScale;
        void Start() { baseScale = pm.transform.localScale; }

        void Update()
        {
            if (run != null && run.Running) runMemory = Time.time + 0.6f;
            bool fwd = Time.time < runMemory;
            float power = 0; bool crouch = false;

            if (gestures != null && gestures.source != null && gestures.source.TryGet(out var f) && f.LeftHip.valid && f.RightHip.valid)
            {
                float hipY = (f.LeftHip.p.y + f.RightHip.p.y) * 0.5f;           // y cresce para baixo
                if (baseY < 0) baseY = hipY; else baseY = Mathf.Lerp(baseY, hipY, Time.deltaTime * 0.15f);
                crouch = hipY - baseY > crouchDrop;
                // joelho dobrado: o joelho sobe/avanca ate perto da altura do quadril
                foreach (var k in new[] { (f.LeftKnee, f.LeftHip, f.LeftAnkle), (f.RightKnee, f.RightHip, f.RightAnkle) })
                    if (k.Item1.valid && k.Item2.valid && k.Item3.valid)
                    {
                        float thigh = k.Item1.p.y - k.Item2.p.y, shin = k.Item3.p.y - k.Item1.p.y;
                        if (shin > 0.01f && thigh / (thigh + shin) < 0.28f) crouch = true;
                    }
                float dt = Mathf.Max(f.time - lastT, 0.001f); float vUp = (lastY - hipY) / dt; lastY = hipY; lastT = f.time;
                if (vUp > jumpRise && Time.time > cd) power = Mathf.Clamp01((vUp - jumpRise) / 1.5f + 0.35f);
            }
            else
            {
                // fallback: acelerometro. Impulso para cima forte = pulo; magnitude define a forca.
                float a = Input.acceleration.magnitude; accBase = Mathf.Lerp(accBase, a, Time.deltaTime * 2f);
                float spike = a - accBase;
                if (spike > 0.55f && Time.time > cd) power = Mathf.Clamp01(spike / 1.6f);
                // agachar: o celular DESCE (aceleracao abaixo do normal por instantes). Olhar para baixo nao conta.
                if (spike < -0.3f && Time.time - jumpAt > 1.2f && Time.time > cd) crouchUntil = Time.time + 1.2f;
                if (power > 0f) crouchUntil = 0;
                crouch = Time.time < crouchUntil;
            }
            if (power > 0f) { pm.VrJump = power < 0.45f ? 0.4f : power < 0.8f ? 0.7f : 1f; pm.VrJumpForward = fwd && power > 0.45f; cd = Time.time + 0.6f; jumpAt = Time.time; Jumped?.Invoke(); }
            if (crouch && !wasCrouch) Crouched?.Invoke(); wasCrouch = crouch;
            pm.VrCrouch = crouch;
            crouchAmt = Mathf.MoveTowards(crouchAmt, crouch ? 1f : 0f, Time.deltaTime * 5f);
            pm.transform.localScale = new Vector3(baseScale.x, baseScale.y * Mathf.Lerp(1f, 0.6f, crouchAmt), baseScale.z);
        }
    }
}
