using UnityEngine;
namespace SpiderVR
{
    /// <summary>Suas maos (vistas pela camera) aparecem em primeira pessoa como luvas do traje: voce ve onde elas estao antes de atirar a teia.</summary>
    public class FirstPersonHands : MonoBehaviour
    {
        public GestureRecognizer gestures; public Transform head;
        Transform[] h = new Transform[2];
        void Start()
        {
            for (int i = 0; i < 2; i++)
            {
                var g = GameObject.CreatePrimitive(PrimitiveType.Sphere); Destroy(g.GetComponent<Collider>());
                g.transform.SetParent(head, false); g.transform.localScale = Vector3.one * 0.09f; g.GetComponent<Renderer>().material.color = i == 0 ? new Color(0.1f, 0.2f, 0.8f) : new Color(0.8f, 0.1f, 0.1f);
                g.SetActive(false); h[i] = g.transform;
            }
        }
        void Update()
        {
            bool ok = gestures != null && gestures.source != null && gestures.source.TryGet(out var f);
            if (!ok) { h[0].gameObject.SetActive(false); h[1].gameObject.SetActive(false); return; }
            gestures.source.TryGet(out var fr);
            Place(0, fr.LeftWrist, fr.LeftHandSeen); Place(1, fr.RightWrist, fr.RightHandSeen);
        }
        void Place(int i, BodyJoint j, bool seen)
        {
            h[i].gameObject.SetActive(seen && j.valid);
            if (!seen || !j.valid) return;
            var target = new Vector3((j.p.x - 0.5f) * 0.9f, (0.5f - j.p.y) * 0.6f, 0.45f + Mathf.Clamp(-j.p.z * 0.05f, -0.1f, 0.4f));
            h[i].localPosition = Vector3.Lerp(h[i].localPosition, target, 0.5f);
        }
    }
}
