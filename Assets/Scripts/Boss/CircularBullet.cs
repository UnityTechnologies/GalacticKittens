using Unity.Netcode;
using UnityEngine;

public class CircularBullet : NetworkBehaviour
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
            GameObject go = Instantiate(
                _smallBulletPrefab,
                firePosition.position,
                firePosition.rotation);

            go.GetComponent<NetworkObject>().Spawn();
        }

        // De-spawn me
        NetworkObjectDespawner.DespawnNetworkObject(NetworkObject);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        // After a random time amount, blow up and spawn small bullets
        float randomSpawn = Random.Range(1.5f, 3f);
        Invoke(nameof(SpawnBullets), randomSpawn);

        base.OnNetworkSpawn();
    }
}
