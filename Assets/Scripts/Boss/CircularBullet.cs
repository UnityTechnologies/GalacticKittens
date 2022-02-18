using Unity.Netcode;
using UnityEngine;

public class CircularBullet : NetworkBehaviour
{
    [SerializeField]
    private float _speed;
    [SerializeField]
    private GameObject _smallBulletPrefab;
    [SerializeField]
    private Transform[] _firePositions;

    private void Update()
    {
        if (IsServer)
        {
            transform.Translate(Vector3.left * _speed * Time.deltaTime);
        }
    }

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
            // After x time blow up and spawn small bullets        
            float randomSpawn = Random.Range(1.5f, 3f);
            Invoke("SpawnBullets", randomSpawn);
        }
        base.OnNetworkSpawn();
    }
}
