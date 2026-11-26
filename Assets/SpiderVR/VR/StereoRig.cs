using UnityEngine;

namespace SpiderVR
{
    /// <summary>Camera dupla (tela dividida) para VR Box. Cabeca vem do giroscopio ou da camera conforme o menu Opcoes.</summary>
    public class StereoRig : MonoBehaviour
    {
        public Transform head;
        public float ipd = 0.064f;
        public float fov = 90f;
        Camera left, right; IHeadTracker tracker; CameraHeadTracker camTracker;

        void Start()
        {
            if (head == null) head = transform;
            var main = GetComponent<Camera>();
            if (main) main.enabled = false;
            left = MakeEye("Left", new Rect(0, 0, 0.5f, 1), -ipd / 2);
            right = MakeEye("Right", new Rect(0.5f, 0, 0.5f, 1), ipd / 2);
            if (GameSettings.Tracking == TrackingMode.Camera) { camTracker = CameraHeadTracker.Shared; tracker = camTracker; }
            else tracker = new GyroHeadTracker();
            tracker.Begin(); tracker.Recenter();
        }

        Camera MakeEye(string n, Rect r, float x)
        {
            var go = new GameObject("Eye_" + n); go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(x, 0, 0);
            var c = go.AddComponent<Camera>();
            c.rect = r; c.fieldOfView = fov; c.nearClipPlane = 0.1f; c.farClipPlane = 600f;
            c.allowHDR = false; c.allowMSAA = false; c.clearFlags = CameraClearFlags.Skybox;
            return c;
        }

        void Update()
        {
            if (camTracker != null) camTracker.Tick();
            if (tracker != null) head.localRotation = tracker.Rotation;
            if (Input.touchCount >= 4) tracker.Recenter();
        }
        void OnDestroy() { tracker?.End(); }
    }
}
