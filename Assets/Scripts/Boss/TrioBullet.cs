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
            GameObject go = Instantiate(_smallBulletPrefab, firePosition.position, firePosition.rotation);
            go.GetComponent<NetworkObject>().Spawn();
        }

        // Despawn me
        NetworkObject.Despawn();
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
