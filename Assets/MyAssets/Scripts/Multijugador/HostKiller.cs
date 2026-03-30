using UnityEngine;
using Unity.Netcode;

public class HostKiller : NetworkBehaviour
{
    private ExitCube exitCube;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsHost)
        {
            exitCube = FindAnyObjectByType<ExitCube>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsHost) return;

        NetworkObject victimNet = other.GetComponentInParent<NetworkObject>();
        if (victimNet == null) return;
        if (victimNet.OwnerClientId == NetworkManager.ServerClientId) return;
        if (!victimNet.IsPlayerObject) return;

        if (exitCube != null)
        {
            exitCube.HostKillPlayerRpc(victimNet.OwnerClientId);
        }
    }
}
