using UnityEngine;

namespace SpiderVR
{
    public enum TrackingMode { Gyro = 0, Camera = 1 }
    public enum WebMode { Auto = 0, Manual = 1 }
    public enum LocomotionMode { Shake = 0, RealSteps = 1, Marching = 2 }

    /// <summary>Configuracoes salvas (menu Opcoes): giroscopio ou camera, FPS alvo, escala de resolucao.</summary>
    public static class GameSettings
    {
        public static TrackingMode Tracking
        {
            get => (TrackingMode)PlayerPrefs.GetInt("svr_tracking", 0);
            set { PlayerPrefs.SetInt("svr_tracking", (int)value); PlayerPrefs.Save(); }
        }
        public static LocomotionMode Locomotion
        {
            get => (LocomotionMode)PlayerPrefs.GetInt("svr_loco", 0);
            set { PlayerPrefs.SetInt("svr_loco", (int)value); PlayerPrefs.Save(); }
        }
        public static WebMode Web
        {
            get => (WebMode)PlayerPrefs.GetInt("svr_web", 0);
            set { PlayerPrefs.SetInt("svr_web", (int)value); PlayerPrefs.Save(); }
        }
        public static string SuitId
        {
            get => PlayerPrefs.GetString("svr_suit", "");
            set { PlayerPrefs.SetString("svr_suit", value); PlayerPrefs.Save(); }
        }
        public static float ShakeSensitivity
        {
            get => PlayerPrefs.GetFloat("svr_shake", 1f);
            set { PlayerPrefs.SetFloat("svr_shake", Mathf.Clamp(value, 0.3f, 2f)); PlayerPrefs.Save(); }
        }
        public static bool TutorialDone
        {
            get => PlayerPrefs.GetInt("svr_tut", 0) == 1;
            set { PlayerPrefs.SetInt("svr_tut", value ? 1 : 0); PlayerPrefs.Save(); }
        }
        public static bool BodyTracking
        {
            get => PlayerPrefs.GetInt("svr_body", 1) == 1;
            set { PlayerPrefs.SetInt("svr_body", value ? 1 : 0); PlayerPrefs.Save(); }
        }
        public static int TargetFps
        {
            get => PlayerPrefs.GetInt("svr_fps", 60);
            set { PlayerPrefs.SetInt("svr_fps", value); PlayerPrefs.Save(); }
        }
        public static float ResolutionScale
        {
            get => PlayerPrefs.GetFloat("svr_res", 0.7f);
            set { PlayerPrefs.SetFloat("svr_res", Mathf.Clamp(value, 0.4f, 1f)); PlayerPrefs.Save(); }
        }
        public static string PlayerName
        {
            get => PlayerPrefs.GetString("svr_name", "Spider" + Random.Range(100, 999));
            set { PlayerPrefs.SetString("svr_name", value); PlayerPrefs.Save(); }
        }
    }
}
