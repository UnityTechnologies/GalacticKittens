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
        // Safety check
        if (networkObject != null && networkObject.IsSpawned)
            networkObject.Despawn();
    }
}