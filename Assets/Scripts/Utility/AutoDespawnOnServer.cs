using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class AutoDespawnOnServer : NetworkBehaviour
{
    [Min(0f)]
    [SerializeField]
    [Header("in seconds")]
    private float m_autoDestroyTime;

    private void Update()
    {
        if (!IsServer)
            return;

        m_autoDestroyTime -= Time.deltaTime;

        if(m_autoDestroyTime <= 0f)
        {
            Despawn();
        }
    }

    private void Despawn()
    {
        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }

    public override void OnNetworkSpawn()
    {
        // we only will Despawn on the server, so no need to have this active on client-side
        if (!IsServer)
            enabled = false;
    }
}