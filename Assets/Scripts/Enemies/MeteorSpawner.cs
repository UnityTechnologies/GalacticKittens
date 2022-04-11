using Unity.Netcode;
using UnityEngine;

public class MeteorSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject m_meteorPrefab;

    [SerializeField]
    private float m_spawingTime = 2f;

    private float m_timer;

    private void Update()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            m_timer += Time.deltaTime;
            if (m_timer > m_spawingTime)
            {
                m_timer = 0f;
                SpawnMeteor();
            }
        }
    }

    private void SpawnMeteor()
    {
        // The min and max Y pos for spawning the meteors
        float randomYpos = Random.Range(-5.0f, 6.0f);
        Vector3 newMeteorPosition = new Vector3(transform.position.x, randomYpos, 0f);

        NetworkObjectSpawner.SpawnNewNetworkObject(m_meteorPrefab, newMeteorPosition);
    }

}
