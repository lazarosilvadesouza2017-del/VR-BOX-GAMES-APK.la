using UnityEngine;

namespace SpiderVR
{
    /// <summary>Ajusta FPS/resolucao/qualidade para celulares fracos e reduz resolucao sozinho se o FPS cair.</summary>
    public class PerformanceManager : MonoBehaviour
    {
        float scale, acc; int frames;

        void Awake()
        {
            Application.targetFrameRate = GameSettings.TargetFps;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            scale = GameSettings.ResolutionScale;
            bool low = SystemInfo.systemMemorySize < 3500 || SystemInfo.processorCount <= 4;
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.antiAliasing = 0;
            QualitySettings.softParticles = false;
            QualitySettings.lodBias = low ? 0.6f : 1f;
            QualitySettings.skinWeights = SkinWeights.TwoBones;
            QualitySettings.pixelLightCount = low ? 0 : 1;
            QualitySettings.masterTextureLimit = low ? 1 : 0;
            if (low) scale = Mathf.Min(scale, 0.6f);
            Apply();
        }

        void Update()
        {
            acc += Time.unscaledDeltaTime; frames++;
            if (acc < 3f) return;
            float fps = frames / acc; acc = 0; frames = 0;
            float target = GameSettings.TargetFps;
            if (fps < target * 0.8f && scale > 0.45f) { scale -= 0.05f; Apply(); }
            else if (fps > target * 0.97f && scale < GameSettings.ResolutionScale) { scale += 0.05f; Apply(); }
        }

        void Apply()
        {
            int h = Mathf.RoundToInt(Display.main.systemHeight * scale);
            int w = Mathf.RoundToInt(Display.main.systemWidth * scale);
            Screen.SetResolution(w, h, true);
        }
    }
}
