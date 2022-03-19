using Unity.Netcode;
using UnityEngine;

// This script is a helper for spawning on the network

public class NetworkSpawnController : MonoBehaviour
{  
    //TODO: TEST function, evaluate if needed
    public static GameObject SpawnHelper(
        GameObject prefab,
        Vector3? position = null,
        Quaternion? rotation = null,
        bool destroyWithScene = true,
        bool changeOwnershipToClient = false,
        ulong newClientOwnerId = 0UL)
    {
        if (position == null)
            position = Vector3.zero;

        if (rotation == null)
            rotation = Quaternion.identity;

        if (NetworkManager.Singleton.IsServer)
        {
            GameObject newGameObject = Instantiate(prefab, position.Value, rotation.Value);

            NetworkObject newGameObjectNetworkObject = newGameObject.GetComponent<NetworkObject>();
            newGameObjectNetworkObject.Spawn(destroyWithScene);

            if (changeOwnershipToClient && newClientOwnerId != 0UL)
            {
                //TODO: NOT THIS, BECAUSE IT WORKS DUE TO A CURRENT BUG IN NETCODE!!
                newGameObjectNetworkObject.ChangeOwnership(newClientOwnerId);
            }

            return newGameObject;
        }

        return null;
    }
}