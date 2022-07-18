using Unity.Netcode;
using UnityEngine;

public class NetworkObjectSpawner
{
    public static GameObject SpawnNewNetworkObject(
        GameObject prefab,
        bool destroyWithScene = true)
    {
#if UNITY_EDITOR
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogError("ERROR: Spawning not happening in the server!");
        }
#endif
        GameObject newGameObject = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);

        NetworkObject newGameObjectNetworkObject = newGameObject.GetComponent<NetworkObject>();
        newGameObjectNetworkObject.Spawn(destroyWithScene);

        return newGameObject;
    }

    public static GameObject SpawnNewNetworkObject(
        GameObject prefab,
        Vector3 position,
        bool destroyWithScene = true)
    {
#if UNITY_EDITOR
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogError("ERROR: Spawning not happening in the server!");
        }
#endif
        GameObject newGameObject = Object.Instantiate(prefab, position, Quaternion.identity);

        NetworkObject newGameObjectNetworkObject = newGameObject.GetComponent<NetworkObject>();
        newGameObjectNetworkObject.Spawn(destroyWithScene);

        return newGameObject;
    }

    public static GameObject SpawnNewNetworkObject(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation,
        bool destroyWithScene = true)
    {
#if UNITY_EDITOR
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogError("ERROR: Spawning not happening in the server!");
        }
#endif
        GameObject newGameObject = Object.Instantiate(prefab, position, rotation);

        NetworkObject newGameObjectNetworkObject = newGameObject.GetComponent<NetworkObject>();
        newGameObjectNetworkObject.Spawn(destroyWithScene);

        return newGameObject;
    }

    public static GameObject SpawnNewNetworkObjectChangeOwnershipToClient(
        GameObject prefab,
        Vector3 position,
        ulong newClientOwnerId,
        bool destroyWithScene = true)
    {
#if UNITY_EDITOR
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogError("ERROR: Spawning not happening in the server!");
        }
#endif
        GameObject newGameObject = Object.Instantiate(prefab, position, Quaternion.identity);

        NetworkObject newGameObjectNetworkObject = newGameObject.GetComponent<NetworkObject>();
        newGameObjectNetworkObject.SpawnWithOwnership(newClientOwnerId, destroyWithScene);

        return newGameObject;
    }
}