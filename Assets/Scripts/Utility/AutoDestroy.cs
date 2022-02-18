using Unity.Netcode;
using UnityEngine;

public class AutoDestroy : NetworkBehaviour
{
    [SerializeField]
    private float _autoDestroyTime;

    private void AutoDestroyMe()
    {
        if(NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            Invoke(nameof(AutoDestroyMe), _autoDestroyTime);

        base.OnNetworkSpawn();
    }
}