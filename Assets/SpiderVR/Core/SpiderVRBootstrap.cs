using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpiderVR
{
    /// <summary>
    /// Liga tudo na cena "Main Game" sem editar a cena na mao: acha Player/Camera Holder do Swinging Simulator,
    /// troca o mouse (FirstPerson) por cabeca VR, desliga o WebShooters antigo e adiciona teia por gesto, soco/chute e otimizador.
    /// </summary>
    public static class SpiderVRBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Run()
        {
            var pm = Object.FindObjectOfType<PlayerMovement>();
            if (pm == null)
            {
                if (SceneManager.GetActiveScene().name.Contains("Menu") && Object.FindObjectOfType<MainMenuVR>() == null)
                    new GameObject("MenuVR").AddComponent<MainMenuVR>();
                return;
            }
            new GameObject("SpiderVR_Core").AddComponent<PerformanceManager>();
            var cam = Camera.main; var fp = cam ? cam.GetComponent<FirstPerson>() : null;
            if (fp) { fp.enabled = false; Cursor.lockState = CursorLockMode.None; }
            var rig = cam.gameObject.AddComponent<StereoRig>(); rig.head = fp ? fp.camHolder : cam.transform;
            var gr = pm.gameObject.AddComponent<GestureRecognizer>();
            if (GameSettings.BodyTracking) { gr.source = new MediaPipePoseSource(); gr.source.Begin(); }
            var old = Object.FindObjectOfType<WebShooters>(); if (old) { old.enabled = false; old.gestures = gr; old.enabled = true; }
            var pull = pm.gameObject.AddComponent<WebPull>(); pull.gestures = gr; pull.head = cam.transform; pull.wrist = old ? old.wrist : null;
            var mel = pm.gameObject.AddComponent<MeleeCombat>(); mel.gestures = gr; mel.head = cam.transform;

            var orient = fp ? fp.orientation : pm.transform;
            var run = pm.gameObject.AddComponent<RunLocomotion>(); run.pm = pm; run.orientation = orient; run.head = cam.transform; run.gestures = gr;
            var tut = pm.gameObject.AddComponent<TutorialVR>(); tut.head = cam.transform; tut.gestures = gr; tut.run = run;
            var bar = pm.gameObject.AddComponent<WebBarrier>(); bar.gestures = gr; bar.head = cam.transform;
            var jc = pm.gameObject.AddComponent<JumpCrouch>(); jc.pm = pm; jc.run = run; jc.gestures = gr; jc.head = cam.transform;
            var gun = pm.gameObject.AddComponent<GunHandler>(); gun.gestures = gr; gun.head = cam.transform;
            if (pm.GetComponent<PlayerHealth>() == null) pm.gameObject.AddComponent<PlayerHealth>();
            var bs = new GameObject("Bandidos").AddComponent<BanditSpawner>(); bs.player = pm.transform;
            var hands = pm.gameObject.AddComponent<FirstPersonHands>(); hands.gestures = gr; hands.head = cam.transform;
            var sel = new GameObject("SuitSelect").AddComponent<SuitSelectUI>(); sel.pm = pm;
            var city = pm.gameObject.AddComponent<CityOptimizer>(); city.player = pm.transform; city.skylinePng = Resources.Load<Texture2D>("Skyline"); city.renderDistance = GameSettings.TargetFps == 30 ? 140f : 220f;
        }
    }
}
