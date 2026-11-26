using System;
using UnityEngine;
namespace SpiderVR
{
    public enum Side { Left, Right }

    /// <summary>
    /// Transforma PoseFrame em acoes: gesto de teia (indicador+minimo esticados, medio+anelar dobrados),
    /// soco (punho avanca rapido) e chute (tornozelo sobe/avanca rapido). Sem deteccao => usa toque na tela.
    /// </summary>
    public class GestureRecognizer : MonoBehaviour
    {
        public event Action<Side> WebStart, WebEnd, Punch, Kick, GunStart, GunEnd, Yank;
        public float punchSpeed = 1.6f, kickSpeed = 1.4f, cooldown = 0.35f;
        public IPoseSource source;
        PoseFrame last; bool[] web = new bool[2], gun = new bool[2]; float[] cd = new float[6];
        float webHold;

        void Update()
        {
            // fallback por toque (VR Box tem botao que gera toque): segurar = teia
            if (source == null || !source.TryGet(out var f)) { TouchFallback(); return; }
            Hand(f.LeftFingersUp, f.LeftHandSeen, Side.Left);
            Hand(f.RightFingersUp, f.RightHandSeen, Side.Right);
            if (last != null)
            {
                float dt = Mathf.Max(f.time - last.time, 0.001f);
                Strike(f.LeftWrist, last.LeftWrist, dt, 0, Side.Left, Punch, punchSpeed);
                Strike(f.RightWrist, last.RightWrist, dt, 1, Side.Right, Punch, punchSpeed);
                YankCheck(f.LeftWrist, last.LeftWrist, dt, 4, Side.Left);
                YankCheck(f.RightWrist, last.RightWrist, dt, 5, Side.Right);
                Strike(f.LeftAnkle, last.LeftAnkle, dt, 2, Side.Left, Kick, kickSpeed);
                Strike(f.RightAnkle, last.RightAnkle, dt, 3, Side.Right, Kick, kickSpeed);
            }
            last = f;
        }

        void Hand(bool[] up, bool seen, Side s)
        {
            bool isWeb = seen && up[0] && !up[1] && !up[2] && up[3];
            int i = (int)s;
            bool isGun = seen && up[0] && up[4] && !up[1] && !up[2] && !up[3];
            if (isGun && !gun[i]) { gun[i] = true; GunStart?.Invoke(s); } else if (!isGun && gun[i]) { gun[i] = false; GunEnd?.Invoke(s); }
            if (isWeb && !web[i]) { web[i] = true; WebStart?.Invoke(s); }
            else if (!isWeb && web[i]) { web[i] = false; WebEnd?.Invoke(s); }
        }

        void Strike(BodyJoint now, BodyJoint prev, float dt, int slot, Side s, Action<Side> ev, float thr)
        {
            if (!now.valid || !prev.valid || Time.time < cd[slot]) return;
            // velocidade em "tamanhos de tela por segundo" + avanco em profundidade (z negativo = perto da camera)
            Vector3 v = (now.p - prev.p) / dt;
            float speed = new Vector2(v.x, v.y).magnitude + Mathf.Max(0, -v.z);
            if (speed > thr) { cd[slot] = Time.time + cooldown; ev?.Invoke(s); }
        }

        // puxao: mao desce/recua rapido (so conta se for mais vertical que um soco)
        void YankCheck(BodyJoint now, BodyJoint prev, float dt, int slot, Side s)
        {
            if (!now.valid || !prev.valid || Time.time < cd[slot]) return;
            float vy = (now.p.y - prev.p.y) / dt;   // y para baixo
            if (vy > 1.8f) { cd[slot] = Time.time + 0.5f; Yank?.Invoke(s); }
        }

        void TouchFallback()
        {
            int n = Input.touchCount; if (n == 0 && Input.GetMouseButton(0)) n = 1;
            bool l = n == 1, r = n == 2;
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(0).deltaPosition.y < -40f && Time.time > cd[4]) { cd[4] = Time.time + 0.5f; Yank?.Invoke(Side.Right); }
            if (l && !web[0]) { web[0] = true; WebStart?.Invoke(Side.Left); } else if (!l && web[0]) { web[0] = false; WebEnd?.Invoke(Side.Left); }
            if (r && !web[1]) { web[1] = true; WebStart?.Invoke(Side.Right); } else if (!r && web[1]) { web[1] = false; WebEnd?.Invoke(Side.Right); }
        }
    }
}
