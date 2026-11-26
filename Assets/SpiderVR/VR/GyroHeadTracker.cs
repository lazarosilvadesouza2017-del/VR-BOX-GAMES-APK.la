using UnityEngine;
namespace SpiderVR
{
    public class GyroHeadTracker : IHeadTracker
    {
        Quaternion offset = Quaternion.identity;
        static Quaternion Raw()
        {
            var g = Input.gyro.attitude;
            return Quaternion.Euler(90f, 0f, 0f) * new Quaternion(g.x, g.y, -g.z, -g.w);
        }
        public Quaternion Rotation => offset * Raw();
        public void Begin() { Input.gyro.enabled = true; }
        public void End() { Input.gyro.enabled = false; }
        public void Recenter() { offset = Quaternion.Euler(0f, -Raw().eulerAngles.y, 0f); }
    }
}
