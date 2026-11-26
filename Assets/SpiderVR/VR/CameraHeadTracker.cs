using UnityEngine;

namespace SpiderVR
{
    /// <summary>
    /// Sensor de movimento pela camera (sem giroscopio), leve: imagem 64x48 em cinza, block-matching em 5 regioes.
    ///  - Rotacao da cabeca (yaw/pitch): deslocamento medio da imagem a ~30 Hz.
    ///  - Aproximacao/andar: DIVERGENCIA do fluxo (bordas indo para fora = voce avancando); rotacao e deslocamento uniforme se cancelam.
    /// Limitacoes: precisa de cena com textura e luz, nao mede roll, a rotacao acumula deriva (4 dedos recentraliza).
    /// </summary>
    public class CameraHeadTracker : IHeadTracker
    {
        static CameraHeadTracker shared; public static CameraHeadTracker Shared => shared ?? (shared = new CameraHeadTracker());
        const int W = 64, H = 48, R = 4, P = 16;
        WebCamTexture cam; Color32[] buf; byte[] prev = new byte[W * H], cur = new byte[W * H], refF = new byte[W * H];
        float yaw, pitch, nextSample; int lastFrame = -1, users, sampleCount; bool hasPrev, hasRef;
        public float HorizontalFovDeg = 60f, Sign = 1f, ApproachSign = 1f, ApproachThreshold = 2.5f;
        public bool UseFrontCamera = false;
        public WebCamTexture Camera => cam;
        public float Divergence { get; private set; }
        public bool Approaching { get; private set; }

        public Quaternion Rotation => Quaternion.Euler(pitch, yaw, 0f);
        public void Recenter() { yaw = pitch = 0; }

        public void Begin()
        {
            if (users++ > 0) return;
            foreach (var d in WebCamTexture.devices)
                if (d.isFrontFacing == UseFrontCamera) { cam = new WebCamTexture(d.name, 320, 240, 30); break; }
            if (cam == null && WebCamTexture.devices.Length > 0) cam = new WebCamTexture(WebCamTexture.devices[0].name, 320, 240, 30);
            if (cam != null) cam.Play();
        }
        public void End() { if (--users <= 0 && cam != null) { cam.Stop(); users = 0; } }

        public void Tick()
        {
            if (lastFrame == Time.frameCount) return; lastFrame = Time.frameCount;
            if (cam == null || !cam.isPlaying || !cam.didUpdateThisFrame || Time.unscaledTime < nextSample) return;
            nextSample = Time.unscaledTime + 1f / 30f;
            if (buf == null || buf.Length != cam.width * cam.height) buf = new Color32[cam.width * cam.height];
            cam.GetPixels32(buf);
            int sw = cam.width, sh = cam.height;
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                { var c = buf[(y * sh / H) * sw + (x * sw / W)]; cur[y * W + x] = (byte)((c.r + c.g + c.b) / 3); }

            if (hasPrev)
            {
                Vector2 f = Flow(cur, prev, 24, 16);                 // centro: rotacao
                yaw += Sign * f.x * HorizontalFovDeg / W; pitch = Mathf.Clamp(pitch + Sign * f.y * HorizontalFovDeg / W, -85f, 85f);
            }
            System.Buffer.BlockCopy(cur, 0, prev, 0, cur.Length); hasPrev = true;

            if (++sampleCount % 6 == 0)                               // baseline de 0,2 s: expansao fica visivel em pixels inteiros
            {
                if (hasRef)
                {
                    float d = (Flow(cur, refF, 42, 16).x - Flow(cur, refF, 6, 16).x) + (Flow(cur, refF, 24, 28).y - Flow(cur, refF, 24, 4).y);
                    Divergence = Mathf.Lerp(Divergence, ApproachSign * d, 0.5f);
                    Approaching = Approaching ? Divergence > ApproachThreshold * 0.6f : Divergence > ApproachThreshold;
                }
                System.Buffer.BlockCopy(cur, 0, refF, 0, cur.Length); hasRef = true;
            }
        }

        static Vector2 Flow(byte[] a, byte[] b, int x0, int y0)
        {
            int best = int.MaxValue, bx = 0, by = 0;
            for (int dy = -R; dy <= R; dy++)
                for (int dx = -R; dx <= R; dx++)
                {
                    int sad = 0;
                    for (int y = 0; y < P; y += 2)
                        for (int x = 0; x < P; x += 2)
                            sad += Mathf.Abs(a[(y0 + y) * W + x0 + x] - b[(y0 + y + dy) * W + x0 + x + dx]);
                    if (sad < best) { best = sad; bx = dx; by = dy; }
                }
            return new Vector2(bx, by);
        }
    }
}
