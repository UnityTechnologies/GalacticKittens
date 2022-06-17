using Unity.Netcode;
using UnityEngine;

public class SimpleEnemySpawner : NetworkBehaviour
{
    public GameObject spaceGhostEnemyPrefabToSpawn;

    public GameObject spaceShooterEnemyPrefabToSpawn;

    [Header("Enemies")]
    [SerializeField]
    private float m_EnemySpawnTime = 1.8f;

    [Header("Meteors")]
    [SerializeField] 
    private GameObject m_meteorPrefab;

    [SerializeField]
    private float m_meteorSpawningTime = 1.2f;

    private Vector3 m_CurrentNewEnemyPosition = new Vector3();
    private float m_CurrentEnemySpawnTime = 0f;

    private Vector3 m_CurrentNewMeteorPosition = new Vector3();
    private float m_CurrentMeteorSpawnTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the enemy and meteor spawn position based on my owning GO's x position
        m_CurrentNewEnemyPosition.x = transform.position.x;
        m_CurrentNewEnemyPosition.z = 0f;

        m_CurrentNewMeteorPosition.x = transform.position.x;
        m_CurrentNewMeteorPosition.z = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer)
            return;
        
        UpdateEnemySpawning();
        
        UpdateMeteorSpawning();
    }
    
    private void UpdateEnemySpawning()
    {
        m_CurrentEnemySpawnTime += Time.deltaTime;
        if (m_CurrentEnemySpawnTime >= m_EnemySpawnTime)
        {
            // update the new enemy's spawn position(y value). This way we don't have to allocate
            // a new Vector3 each time.
            m_CurrentNewEnemyPosition.y = Random.Range(-5f, 5f);

            var nextPrefabToSpawn = GetNextRandomEnemyPrefabToSpawn();

            NetworkObjectSpawner.SpawnNewNetworkObject(
                nextPrefabToSpawn,
                m_CurrentNewEnemyPosition);

            m_CurrentEnemySpawnTime = 0f;
        }
    }

    GameObject GetNextRandomEnemyPrefabToSpawn()
    {
        int randomPick = Random.Range(0, 99);

        if (randomPick < 50)
        {
            return spaceGhostEnemyPrefabToSpawn;
        }

        // randomPick >= 50
        return spaceShooterEnemyPrefabToSpawn;
    }
    
    private void UpdateMeteorSpawning()
    {
        m_CurrentMeteorSpawnTime += Time.deltaTime;
        if (m_CurrentMeteorSpawnTime > m_meteorSpawningTime)
        {
            SpawnNewMeteor();

            m_CurrentMeteorSpawnTime = 0f;
        }
    }

    void SpawnNewMeteor()
    {
        // The min and max Y pos for spawning the meteors
        m_CurrentNewMeteorPosition.y = Random.Range(-5f, 6f);

        NetworkObjectSpawner.SpawnNewNetworkObject(m_meteorPrefab, m_CurrentNewMeteorPosition);
    }
}
