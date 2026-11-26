using UnityEngine;
namespace SpiderVR
{
    /// <summary>Pontos do corpo normalizados (0..1 na imagem, y cresce para BAIXO, z = profundidade relativa).</summary>
    public struct BodyJoint { public Vector3 p; public bool valid; }

    public class PoseFrame
    {
        public BodyJoint LeftWrist, RightWrist, LeftAnkle, RightAnkle, LeftShoulder, RightShoulder, LeftKnee, RightKnee, LeftHip, RightHip, Nose;
        // dedos estendidos: indicador, medio, anelar, minimo, polegar
        public bool[] LeftFingersUp = new bool[5], RightFingersUp = new bool[5];
        public bool LeftHandSeen, RightHandSeen;
        public float time;
    }

    public interface IPoseSource { bool TryGet(out PoseFrame frame); void Begin(); void End(); }
}
