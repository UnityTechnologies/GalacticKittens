using UnityEngine;
using Unity.Netcode;

public static class NetworkObjectDespawner
{
    public static void DespawnNetworkObject(NetworkObject networkObject)
    {
#if UNITY_EDITOR
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogError("ERROR: De-spawning not happening in the server!");
        }
#endif
        // if I'm an active on the networking session, tell all clients to remove
        // the instance that owns this NetworkObject
        if (networkObject != null && networkObject.IsSpawned)
            networkObject.Despawn();
    }
}