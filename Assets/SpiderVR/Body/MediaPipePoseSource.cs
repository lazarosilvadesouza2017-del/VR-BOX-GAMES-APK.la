using UnityEngine;

namespace SpiderVR
{
    /// <summary>
    /// Maos + corpo pela camera usando MediaPipe Tasks (modelos em StreamingAssets). So existe no Android; no editor/PC nao detecta (o jogo usa toque).
    /// Camera de tras (VR Box): enxerga suas maos a sua frente. De frente para um espelho: use a camera frontal (imagem espelhada pelo codigo).
    /// </summary>
    public class MediaPipePoseSource : IPoseSource
    {
        PoseFrame frame = new PoseFrame(); float lastTime = -10;
#if UNITY_ANDROID && !UNITY_EDITOR
        Driver driver;
#endif

        public void Begin()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var go = new GameObject("MediaPipeDriver"); Object.DontDestroyOnLoad(go);
            driver = go.AddComponent<Driver>(); driver.owner = this;
#endif
        }
        public void End()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (driver) Object.Destroy(driver.gameObject);
#endif
        }
        public bool TryGet(out PoseFrame f) { f = frame; return Time.time - lastTime < 0.6f; }

#if UNITY_ANDROID && !UNITY_EDITOR
        class Driver : MonoBehaviour
        {
            public MediaPipePoseSource owner; AndroidJavaObject bridge; CameraHeadTracker cam; byte[] bytes; float next; long lastStamp;

            void Start()
            {
                cam = CameraHeadTracker.Shared; cam.Begin();
                try
                {
                    var act = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
                    bridge = new AndroidJavaObject("com.spidervr.mp.MPBridge", act); bridge.Call("init");
                }
                catch (System.Exception e) { Debug.LogWarning("MediaPipe indisponivel: " + e.Message); enabled = false; }
            }

            void OnDestroy() { if (cam != null) cam.End(); }

            void Update()
            {
                var wc = cam.Camera; if (bridge == null || wc == null || !wc.isPlaying) return;
                if (Time.unscaledTime >= next && wc.didUpdateThisFrame && bridge.Call<bool>("isReady"))
                {
                    next = Time.unscaledTime + 1f / 15f; Send(wc);
                }
                long st = bridge.Call<long>("getStamp");
                if (st != lastStamp) { lastStamp = st; Read(); }
            }

            void Send(WebCamTexture wc)
            {
                int w = wc.width, h = wc.height; if (bytes == null || bytes.Length != w * h * 4) bytes = new byte[w * h * 4];
                var px = wc.GetPixels32(); bool flipY = !wc.videoVerticallyMirrored;   // Unity entrega de baixo para cima
                for (int y = 0; y < h; y++)
                {
                    int sy = flipY ? h - 1 - y : y; int o = y * w * 4;
                    for (int x = 0; x < w; x++) { var c = px[sy * w + x]; bytes[o++] = c.r; bytes[o++] = c.g; bytes[o++] = c.b; bytes[o++] = 255; }
                }
                bridge.Call("submit", bytes, w, h);
            }

            void Read()
            {
                int total = 21, p0 = 0;
                bool front = cam.UseFrontCamera; var f = owner.frame; f.time = Time.time; owner.lastTime = Time.time;
                f.LeftHandSeen = f.RightHandSeen = false;
                float[] hs = bridge.Call<float[]>("hands");
                for (int i = 0; i < 2; i++)
                {
                    int b = i * 65; if (hs[b] < 0.5f) continue;
                    bool right = hs[b + 1] > 0.5f;
                    Vector3[] p = new Vector3[total];
                    for (int j = 0; j < 21; j++) p[j] = new Vector3(front ? 1f - hs[b + 2 + j * 3] : hs[b + 2 + j * 3], hs[b + 3 + j * 3], hs[b + 4 + j * 3]);
                    bool[] up = Fingers(p);
                    var wrist = new BodyJoint { p = new Vector3(p[p0].x, p[p0].y, -Vector2.Distance(p[p0], p[9]) * 5f), valid = true };
                    if (right) { f.RightWrist = wrist; f.RightFingersUp = up; f.RightHandSeen = true; } else { f.LeftWrist = wrist; f.LeftFingersUp = up; f.LeftHandSeen = true; }
                }
                float[] ps = bridge.Call<float[]>("poseLm");
                f.LeftShoulder = J(ps, 11, front); f.RightShoulder = J(ps, 12, front);
                f.LeftHip = J(ps, 23, front); f.RightHip = J(ps, 24, front); f.LeftKnee = J(ps, 25, front); f.RightKnee = J(ps, 26, front);
                f.LeftAnkle = J(ps, 27, front); f.RightAnkle = J(ps, 28, front); f.Nose = J(ps, 0, front);
                if (!f.LeftHandSeen) f.LeftWrist = J(ps, 15, front);
                if (!f.RightHandSeen) f.RightWrist = J(ps, 16, front);
            }

            static BodyJoint J(float[] a, int i, bool front)
            {
                if (a == null || a.Length < (i + 1) * 4 || a[i * 4 + 3] < 0.5f) return default;
                float x = a[i * 4], y = a[i * 4 + 1]; if (x < 0 || x > 1 || y < 0 || y > 1) return default;
                return new BodyJoint { p = new Vector3(front ? 1f - x : x, y, a[i * 4 + 2]), valid = true };
            }

            // dedo esticado = ponta mais longe do pulso que a articulacao do meio; polegar compara com a base do mindinho
            static bool[] Fingers(Vector3[] p)
            {
                int p0 = 0;
                bool Ext(int tip, int pip) => Vector2.Distance(p[tip], p[p0]) > Vector2.Distance(p[pip], p[p0]) * 1.15f;
                return new[] { Ext(8, 6), Ext(12, 10), Ext(16, 14), Ext(20, 18), Vector2.Distance(p[4], p[17]) > Vector2.Distance(p[3], p[17]) * 1.1f };
            }
        }
#endif
    }
}
