using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace SpiderVR
{
    /// <summary>Posicao controlada pelo dono (o jogador local), nao pelo servidor.</summary>
    public class ClientNetworkTransform : NetworkTransform { protected override bool OnIsServerAuthoritative() => false; }

    /// <summary>
    /// Avatar em rede (corpo com traje). O jogador local real e o Player da cena; este objeto so o espelha para os outros.
    /// Quem entra depois nasce ao lado do anfitriao. Socos/teia em outros jogadores viram impulso no dono via RPC.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class PlayerNet : NetworkBehaviour, IDamageable
    {
        Transform local; Rigidbody localRb; GameObject visual;
        readonly NetworkVariable<FixedString64Bytes> suit = new NetworkVariable<FixedString64Bytes>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public void SetSuit(string id) { if (IsOwner && IsSpawned) suit.Value = id; }

        void BuildVisual(string id)
        {
            if (visual) Destroy(visual);
            visual = SuitCatalog.Create(id, transform);
            if (visual == null)   // sem traje escolhido: capsula
            {
                visual = GameObject.CreatePrimitive(PrimitiveType.Capsule); Destroy(visual.GetComponent<Collider>());
                visual.transform.SetParent(transform, false); visual.transform.localPosition = Vector3.up * 0.9f; visual.transform.localScale = new Vector3(0.6f, 0.9f, 0.6f);
            }
            else visual.AddComponent<SuitAnimator>();
            if (IsOwner) foreach (var r in visual.GetComponentsInChildren<Renderer>()) r.enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            var pm = FindObjectOfType<PlayerMovement>(); if (pm) { local = pm.transform; localRb = pm.GetComponent<Rigidbody>(); }
            suit.OnValueChanged += (o, n) => BuildVisual(n.ToString());
            if (IsOwner)
            {
                foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;   // primeira pessoa: o corpo do dono fica invisivel
                suit.Value = GameSettings.SuitId;
                if (!IsHost) StartCoroutine(SpawnNearHost());
            }
            BuildVisual(suit.Value.ToString());
        }

        IEnumerator SpawnNearHost()
        {
            for (int i = 0; i < 100; i++)
            {
                foreach (var p in FindObjectsOfType<PlayerNet>())
                    if (p.OwnerClientId == NetworkManager.ServerClientId && local) { local.position = p.transform.position + p.transform.right * 2f + Vector3.up * 0.5f; yield break; }
                yield return new WaitForSeconds(0.1f);
            }
        }

        void LateUpdate() { if (IsOwner && local) transform.SetPositionAndRotation(local.position, local.rotation); }

        public void Hit(float dmg, Vector3 dir) { if (IsSpawned && !IsOwner) ImpulseServerRpc(dir.normalized * Mathf.Min(dmg * 0.4f, 12f)); }
        public void RequestImpulse(Vector3 v) { if (IsSpawned && !IsOwner) ImpulseServerRpc(v); }

        [ServerRpc(RequireOwnership = false)]
        void ImpulseServerRpc(Vector3 v) => ImpulseClientRpc(v, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } } });

        [ClientRpc]
        void ImpulseClientRpc(Vector3 v, ClientRpcParams p = default) { if (localRb) localRb.AddForce(v, ForceMode.VelocityChange); }
    }
}
