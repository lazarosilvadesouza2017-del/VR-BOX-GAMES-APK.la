using UnityEngine;

namespace SpiderVR
{
    /// <summary>
    /// Maos + corpo pela camera usando MediaPipe Tasks (modelos em StreamingAssets).
    /// Camera de tras (VR Box): enxerga suas maos a sua frente. De frente (mirror): espelha.
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

        public bool TryGet(out PoseFrame f) { f = frame; return Time.time - lastTime < 0.5f; }

#if UNITY_ANDROID && !UNITY_EDITOR
        class Driver : MonoBehaviour
        {
            public MediaPipePoseSource owner; AndroidJavaObject bridge; CameraHeadTracker cam; long lastStamp; float next; byte[] bytes;

            void Start()
            {
                cam = CameraHeadTracker.Shared; cam.Begin();
                try
                {
                    var act = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
                    bridge = new AndroidJavaObject("com.spidervr.mp.MPBridge", act);
                }
                catch (System.Exception e) { Debug.LogWarning("MediaPipe: " + e.Message); }
            }

            void OnDestroy() { if (cam != null) cam.End(); }

            void Update()
            {
                var wc = cam.Camera; if (bridge == null || wc == null || !wc.isPlaying) return;
                if (Time.unscaledTime >= next && wc.didUpdateThisFrame && wc.width > 16)
                {
                    next = Time.unscaledTime + 1f / 15f; Send(wc);
                }
                long st = bridge.Call<long>("getStamp");
                if (st != lastStamp) { lastStamp = st; Read(); }
            }

            void Send(WebCamTexture wc)
            {
                int w = wc.width, h = wc.height; if (bytes == null || bytes.Length != w * h * 4) bytes = new byte[w * h * 4];
                var px = wc.GetPixels32(); bool flipY = !wc.videoVerticallyMirrored;
                for (int y = 0; y < h; y++)
                {
                    int sy = flipY ? h - 1 - y : y; int o = y * w * 4;
                    for (int x = 0; x < w; x++) { var c = px[sy * w + x]; bytes[o] = c.r; bytes[o + 1] = c.g; bytes[o + 2] = c.b; bytes[o + 3] = c.a; o += 4; }
                }
                bridge.Call("submit", bytes, w, h);
            }

            void Read()
            {
                int total = 21, i0 = 0;
                bool front = cam.UseFrontCamera; var f = owner.frame; f.time = owner.lastTime = Time.time;
                f.LeftHandSeen = f.RightHandSeen = false;
                float[] hs = bridge.Call<float[]>("hands");
                if (hs != null)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        int b = i * 65; if (hs.Length < b + 65 || hs[b] < 0.5f) continue;
                        bool right = hs[b + 1] > 0.5f;
                        Vector3[] p = new Vector3[total];
                        for (int j = 0; j < 21; j++) p[j] = new Vector3(front ? 1f - hs[b + 2 + j * 3] : hs[b + 2 + j * 3], 1f - hs[b + 3 + j * 3], hs[b + 4 + j * 3]);
                        bool[] up = Fingers(p);
                        var wrist = new BodyJoint { p = new Vector3(p[i0].x, p[i0].y, p[i0].z), v = true };
                        if (right) { f.RightWrist = wrist; f.RightFingersUp = up; f.RightHandSeen = true; } else { f.LeftWrist = wrist; f.LeftFingersUp = up; f.LeftHandSeen = true; }
                    }
                }
                float[] ps = bridge.Call<float[]>("poseLm");
                f.LeftShoulder = J(ps, 11, front); f.RightShoulder = J(ps, 12, front);
                f.LeftHip = J(ps, 23, front); f.RightHip = J(ps, 24, front);
                f.LeftAnkle = J(ps, 27, front); f.RightAnkle = J(ps, 28, front);
                if (!f.LeftHandSeen) f.LeftWrist = J(ps, 15, front);
                if (!f.RightHandSeen) f.RightWrist = J(ps, 16, front);
            }

            static BodyJoint J(float[] a, int i, bool front)
            {
                if (a == null || a.Length < (i + 1) * 4 || a[i * 4 + 3] < 0.5f) return default;
                float x = a[i * 4], y = a[i * 4 + 1]; if (x < 0 || x > 1 || y < 0 || y > 1) return default;
                return new BodyJoint { p = new Vector3(front ? 1f - x : x, 1f - y, a[i * 4 + 2]), v = true };
            }

            static bool[] Fingers(Vector3[] p)
            {
                int pulso = 0;
                bool Ext(int tip, int pip) => Vector2.Distance(p[tip], p[pulso]) > Vector2.Distance(p[pip], p[pulso]);
                return new[] { Ext(8, 6), Ext(12, 10), Ext(16, 14), Ext(20, 18) };
            }
        }
#endif
    }
}
