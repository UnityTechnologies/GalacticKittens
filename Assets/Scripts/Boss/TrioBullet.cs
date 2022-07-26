using Unity.Netcode;
using UnityEngine;

public class TrioBullet : NetworkBehaviour
{
    [SerializeField]
    private GameObject _smallBulletPrefab;

    [SerializeField]
    private Transform[] _firePositions;

    private void SpawnBullets()
    {
        // Spawn the bullets
        foreach (Transform firePosition in _firePositions)
        {
            GameObject newBullet = Instantiate(
                _smallBulletPrefab,
                firePosition.position,
                firePosition.rotation);
            
            newBullet.GetComponent<NetworkObject>().Spawn();
        }

        // De-spawn me
        NetworkObjectDespawner.DespawnNetworkObject(NetworkObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnBullets();
        }
        base.OnNetworkSpawn();
    }
}
