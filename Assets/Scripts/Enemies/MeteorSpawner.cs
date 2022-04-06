using Unity.Netcode;
using UnityEngine;

public class MeteorSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject _meteorPrefab;
    [SerializeField]
    private float _spawingTime;

    private float _timer;

    private void Update()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            _timer += Time.deltaTime;
            if (_timer > _spawingTime)
            {
                _timer = 0;
                SpawnMeteor();
            }
        }
    }

    private void SpawnMeteor()
    {
        // The min and max Y pos for spawning the meteors
        float randomYpos = Random.Range(-5.0f, 6.0f);
        Vector3 pos = new Vector3(transform.position.x, randomYpos, 0);
        NetworkObjectSpawner.SpawnNewNetworkObject(_meteorPrefab, pos, Quaternion.identity);
    }

}
