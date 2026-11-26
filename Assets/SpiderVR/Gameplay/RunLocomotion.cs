using UnityEngine;

namespace SpiderVR
{
    /// <summary>
    /// SO CORRER, sem andar. Modos (Opcoes):
    ///  Shake     - celular TREMENDO de verdade (acelerometro, so variacao rapida; olhar para baixo/cima nao conta).
    ///  RealSteps - camera ve o ambiente se aproximando (divergencia do fluxo) => voce esta andando; direcao = para onde a cabeca olha.
    ///  Marching  - camera ve os pes (tornozelos sobem/descem) => marchando. Precisa de pose (MediaPipe) e dos pes visiveis.
    /// </summary>
    public class RunLocomotion : MonoBehaviour
    {
        public PlayerMovement pm; public Transform orientation, head; public GestureRecognizer gestures;
        public float shakeThreshold = 0.22f, ankleSpeed = 0.6f, hold = 0.45f;
        LocomotionMode mode; CameraHeadTracker cam; Vector3 lastAcc; float energy, holdUntil, lastLY, lastRY, lastT;
        public bool Running { get; private set; }

        void Start()
        {
            mode = GameSettings.Locomotion; lastAcc = Input.acceleration;
            if (mode == LocomotionMode.RealSteps) { cam = CameraHeadTracker.Shared; cam.Begin(); }
        }
        void OnDestroy() { if (cam != null) cam.End(); }

        void Update()
        {
            bool go = false;
            switch (mode)
            {
                case LocomotionMode.Shake: go = Shaking(); break;
                case LocomotionMode.RealSteps: cam.Tick(); go = cam.Approaching; break;
                case LocomotionMode.Marching: go = Marching(); break;
            }
            if (go) holdUntil = Time.time + hold;
            Running = Time.time < holdUntil;
            pm.VrForward = Running ? 1f : 0f; pm.VrRun = Running;
            if (Running) { var f = head.forward; f.y = 0; if (f.sqrMagnitude > 0.01f) orientation.rotation = Quaternion.LookRotation(f); }
        }

        bool Shaking()
        {
            Vector3 a = Input.acceleration; float d = (a - lastAcc).magnitude; lastAcc = a;
            energy = Mathf.Lerp(energy, d, 0.25f);                    // so variacao brusca; inclinar a cabeca muda pouco por frame
            return energy > shakeThreshold / GameSettings.ShakeSensitivity;
        }

        bool Marching()
        {
            if (gestures == null || gestures.source == null || !gestures.source.TryGet(out var f)) return false;
            float dt = Mathf.Max(Time.time - lastT, 0.001f); bool go = false;
            if (f.LeftAnkle.valid && lastT > 0 && Mathf.Abs(f.LeftAnkle.p.y - lastLY) / dt > ankleSpeed) go = true;
            if (f.RightAnkle.valid && lastT > 0 && Mathf.Abs(f.RightAnkle.p.y - lastRY) / dt > ankleSpeed) go = true;
            lastLY = f.LeftAnkle.p.y; lastRY = f.RightAnkle.p.y; lastT = Time.time; return go;
        }
    }
}
