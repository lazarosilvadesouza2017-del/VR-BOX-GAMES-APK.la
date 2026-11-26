// Requer pacotes: Netcode for GameObjects, Unity Transport, Unity Authentication, Relay e Lobby
// (ja adicionados em Packages/manifest.json) + projeto ligado ao Unity Gaming Services (Project Settings > Services).
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using UnityEngine;

namespace SpiderVR
{
    /// <summary>Online: sala PUBLICA (aparece na lista) ou PRIVADA (so entra com o codigo). O criador ja comeca a jogar.</summary>
    public class OnlineSession : MonoBehaviour
    {
        public int maxPlayers = 4;
        public string LastJoinCode { get; private set; }
        UnityTransport T => NetworkManager.Singleton.GetComponent<UnityTransport>();

        async Task Init()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized) await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        public async Task<bool> CreateRoom(string roomName, bool isPublic)
        {
            await Init();
            var alloc = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            LastJoinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
            T.SetRelayServerData(new RelayServerData(alloc, "dtls"));
            if (isPublic)
                await LobbyService.Instance.CreateLobbyAsync(roomName, maxPlayers, new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Data = new Dictionary<string, DataObject> { { "code", new DataObject(DataObject.VisibilityOptions.Public, LastJoinCode) } }
                });
            return NetworkManager.Singleton.StartHost();   // host entra no jogo na hora, sem esperar ninguem
        }

        public async Task<List<Lobby>> ListRooms()
        {
            await Init();
            var r = await LobbyService.Instance.QueryLobbiesAsync(new QueryLobbiesOptions { Count = 25 });
            return r.Results;
        }

        public async Task<bool> JoinByCode(string code)
        {
            await Init();
            var a = await RelayService.Instance.JoinAllocationAsync(code.Trim().ToUpper());
            T.SetRelayServerData(new RelayServerData(a, "dtls"));
            return NetworkManager.Singleton.StartClient();
        }

        public Task<bool> JoinLobby(Lobby l) => JoinByCode(l.Data["code"].Value);
    }
}
