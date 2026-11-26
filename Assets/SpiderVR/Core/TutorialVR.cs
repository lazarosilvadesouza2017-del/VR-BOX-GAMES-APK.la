using UnityEngine;

namespace SpiderVR
{
    /// <summary>Tutorial na primeira vez (e pelo menu): textos nos DOIS olhos da VR Box. Cada passo avanca ao cumprir ou em 25 s.</summary>
    public class TutorialVR : MonoBehaviour
    {
        public Transform head; public GestureRecognizer gestures; public RunLocomotion run;
        int step; float stepStart, yawAcc, lastYaw, runTime; bool done; GUIStyle st;
        const int Last = 9;

        void Start()
        {
            if (GameSettings.TutorialDone) { enabled = false; return; }
            lastYaw = head.eulerAngles.y; stepStart = Time.time;
            gestures.WebStart += s => { if (step == 2 && s == Side.Left || step == 3 && s == Side.Right) Next(); };
            gestures.Punch += s => { if (step == 4) Next(); }; gestures.Kick += s => { if (step == 4) Next(); };
            JumpCrouch.Jumped += () => { if (step == 5) Next(); };
            JumpCrouch.Crouched += () => { if (step == 6) Next(); };
            WebBarrier.Joined += () => { if (step == 7) Next(); };
            gestures.GunStart += s => { if (step == 8) Next(); };
        }

        void Next() { step++; stepStart = Time.time; }

        void Update()
        {
            if (done) return;
            if (step == 0) { float y = head.eulerAngles.y; yawAcc += Mathf.Abs(Mathf.DeltaAngle(lastYaw, y)); lastYaw = y; if (yawAcc > 60f) Next(); }
            else if (step == 1) { runTime = run != null && run.Running ? runTime + Time.deltaTime : runTime; if (runTime > 2f) Next(); }
            if (step < Last && Time.time - stepStart > 25f) Next();
            if (step == Last && Time.time - stepStart > 6f) { done = true; GameSettings.TutorialDone = true; }
        }

        string Text()
        {
            string run1 = GameSettings.Locomotion == LocomotionMode.Shake ? "Balance o celular (tremer) para correr" :
                          GameSettings.Locomotion == LocomotionMode.RealSteps ? "Ande de verdade na direcao que olha" : "Marche no lugar (pes visiveis na camera)";
            switch (step)
            {
                case 0: return "BEM-VINDO!\nOlhe ao redor movendo a cabeca";
                case 1: return "CORRER\n" + run1;
                case 2: return "TEIA: BALANCAR\nOlhe para um predio e faca o gesto com a mao esquerda (indicador + mindinho) ou toque 1 dedo";
                case 3: return "TEIA: PUXAR\nOlhe para um carro ou pessoa e faca o gesto com a mao direita (ou toque 2 dedos)";
                case 4: return "SOCO / CHUTE\nDe um soco ou chute de verdade";
                case 5: return "PULAR\nPule de verdade (pulinho, normal ou alto). Correndo + pulo = pulo com impulso";
                case 6: return "AGACHAR\nAgache (ou abaixe o celular rapido)";
                case 7: return "BARREIRA DE TEIA\nTeia em dois predios (uma mao em cada) e JUNTE as maos (ou 3 dedos). So soco/chute quebra";
                case 8: return "ARMA\nPegue uma arma que um bandido largou e faca o gesto de arminha (indicador + polegar)";
                default: return "PRONTO!\n4 dedos na tela = recentralizar a visao";
            }
        }

        void OnGUI()
        {
            if (done) return;
            if (st == null) st = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = Screen.height / 20, wordWrap = true, normal = { textColor = Color.white } };
            string t = Text(); float w = Screen.width * 0.42f, h = Screen.height * 0.5f;
            foreach (float cx in new[] { Screen.width * 0.25f, Screen.width * 0.75f })
            {
                var r = new Rect(cx - w / 2, Screen.height * 0.1f, w, h);
                GUI.color = Color.black; GUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), t, st);
                GUI.color = Color.white; GUI.Label(r, t, st);
            }
        }
    }
}
