using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpiderVR
{
    /// <summary>Menu em OnGUI (sem prefab/cena a montar): Solo, Local, Servidores, Criar Sala, Entrar em Privado, Historia, Opcoes.</summary>
    public class MainMenuVR : MonoBehaviour
    {
        public string gameScene = "Main Game";
        enum Page { Main, Local, List, Create, JoinPrivate, Options, Story }
        Page page; string roomName = "Minha sala", code = ""; bool pub = true, busy; string msg = "";
        OnlineSession online; LocalSession local; LanDiscovery lan; List<Unity.Services.Lobbies.Models.Lobby> rooms = new List<Unity.Services.Lobbies.Models.Lobby>();

        void Awake()
        {
            NetRuntime.Ensure();
            online = gameObject.AddComponent<OnlineSession>(); lan = gameObject.AddComponent<LanDiscovery>();
            local = gameObject.AddComponent<LocalSession>(); local.discovery = lan;
        }

        void OnGUI()
        {
            float s = Screen.height / 540f; GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1));
            float w = Screen.width / s, x = w / 2 - 160;
            GUILayout.BeginArea(new Rect(x, 20, 320, 500));
            if (msg != "") GUILayout.Label(msg);
            switch (page)
            {
                case Page.Main:
                    if (Btn("Jogar Solo")) StartSolo();
                    if (Btn("Multiplayer Local (mesmo Wi-Fi)")) { lan.StartListening(); page = Page.Local; }
                    if (Btn("Lista de Servidores")) { page = Page.List; Refresh(); }
                    if (Btn("Criar Sala Online")) page = Page.Create;
                    if (Btn("Entrar em Servidor Privado")) page = Page.JoinPrivate;
                    if (Btn("Tutorial")) { GameSettings.TutorialDone = false; LoadGame(); }
                    if (Btn("Modo Historia")) page = Page.Story;
                    if (Btn("Opcoes")) page = Page.Options; break;
                case Page.Story: GUILayout.Label("EM DESENVOLVIMENTO\nEste modo sera adicionado depois."); Back(); break;
                case Page.Local:
                    if (Btn("Criar sala local")) { if (local.Host(GameSettings.PlayerName)) LoadGame(); }
                    GUILayout.Label("Jogadores na sua rede:");
                    foreach (var h in lan.Found.Values) if (Btn(h.name + "  (" + h.ip + ")")) { if (local.Join(h)) msg = "Conectando..."; }
                    Back(); break;
                case Page.List:
                    if (Btn("Atualizar")) Refresh();
                    foreach (var r in rooms) if (Btn(r.Name + "  " + r.Players.Count + "/" + r.MaxPlayers)) Join(r);
                    Back(); break;
                case Page.Create:
                    roomName = GUILayout.TextField(roomName); pub = GUILayout.Toggle(pub, pub ? "Publica" : "Privada (so com codigo)");
                    if (Btn("Criar e jogar") && !busy) Create();
                    Back(); break;
                case Page.JoinPrivate:
                    code = GUILayout.TextField(code, 8);
                    if (Btn("Entrar") && !busy) JoinCode();
                    Back(); break;
                case Page.Options:
                    bool cam = GameSettings.Tracking == TrackingMode.Camera;
                    if (Btn("Rastreio da cabeca: " + (cam ? "CAMERA" : "GIROSCOPIO"))) GameSettings.Tracking = cam ? TrackingMode.Gyro : TrackingMode.Camera;
                    var lm = GameSettings.Locomotion;
                    string[] ln = { "TREMER o celular", "PASSOS REAIS (camera ve o ambiente)", "MARCHAR (camera ve os pes)" };
                    if (Btn("Correr com: " + ln[(int)lm])) GameSettings.Locomotion = (LocomotionMode)(((int)lm + 1) % 3);
                    bool man = GameSettings.Web == WebMode.Manual;
                    if (Btn("Teia: " + (man ? "PUXAR (voce puxa)" : "AUTOMATICA"))) GameSettings.Web = man ? WebMode.Auto : WebMode.Manual;
                    GUILayout.Label("Sensibilidade do tremor: " + GameSettings.ShakeSensitivity.ToString("0.0"));
                    GameSettings.ShakeSensitivity = GUILayout.HorizontalSlider(GameSettings.ShakeSensitivity, 0.3f, 2f);
                    GameSettings.BodyTracking = GUILayout.Toggle(GameSettings.BodyTracking, "Rastrear maos/corpo pela camera");
                    GUILayout.Label("FPS alvo: " + GameSettings.TargetFps);
                    GameSettings.TargetFps = GUILayout.Toggle(GameSettings.TargetFps == 30, "30 FPS (aparelho fraco)") ? 30 : 60;
                    GUILayout.Label("Resolucao: " + (int)(GameSettings.ResolutionScale * 100) + "%");
                    GameSettings.ResolutionScale = GUILayout.HorizontalSlider(GameSettings.ResolutionScale, 0.4f, 1f);
                    Back(); break;
            }
            GUILayout.EndArea();
        }

        bool Btn(string t) => GUILayout.Button(t, GUILayout.Height(44));
        void Back() { if (Btn("Voltar")) { lan.StopListening(); msg = ""; page = Page.Main; } }
        void StartSolo() { LoadGame(); }
        void LoadGame() { if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening) NetworkManager.Singleton.SceneManager.LoadScene(gameScene, LoadSceneMode.Single); else SceneManager.LoadScene(gameScene); }
        async void Refresh() { try { rooms = await online.ListRooms(); } catch (System.Exception e) { msg = e.Message; } }
        async void Create()
        {
            busy = true;
            try { if (await online.CreateRoom(roomName, pub)) { GUIUtility.systemCopyBuffer = online.LastJoinCode; LoadGame(); } } catch (System.Exception e) { msg = e.Message; }
            busy = false;
        }
        async void Join(Unity.Services.Lobbies.Models.Lobby l) { try { await online.JoinLobby(l); } catch (System.Exception e) { msg = e.Message; } }
        async void JoinCode() { busy = true; try { await online.JoinByCode(code); } catch (System.Exception e) { msg = e.Message; } busy = false; }
    }
}
