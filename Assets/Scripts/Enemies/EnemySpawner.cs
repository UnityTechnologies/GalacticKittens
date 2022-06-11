using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class EnemySpawner : NetworkBehaviour
{
    public GameObject spaceGhostEnemyPrefabToSpawn;
    public GameObject spaceShooterEnemyPrefabToSpawn;
    //.
    //.
    // Opportunity: Can you think of other types of enemies to create and spawn?

    [Header("Boss")]
    [SerializeField]
    BossUI m_bossUI;

    [SerializeField]
    private GameObject m_bossPrefabToSpawn;

    [SerializeField]
    private Transform m_bossPosition;

    [Header("Enemies")]
    [SerializeField]
    private NetworkVariable<float> m_EnemySpawnTime =
        new NetworkVariable<float>(1.8f, NetworkVariableReadPermission.Everyone);

    [SerializeField]
    private NetworkVariable<float> m_bossSpawnTime =
        new NetworkVariable<float>(75f, NetworkVariableReadPermission.Everyone);

    [Header("Meteors")]
    [SerializeField]
    private GameObject m_meteorPrefab;

    [SerializeField]
    private float m_meteorSpawningTime;

    [Space]
    [SerializeField]
    AudioClip m_warningClip;

    [SerializeField]
    GameObject m_warningUI;

    private Vector3 m_CurrentNewEnemyPosition = new Vector3();
    private float m_CurrentEnemySpawnTime = 0f;
    private Vector3 m_CurrentNewMeteorPosition = new Vector3();
    private float m_CurrentMeteorSpawnTime = 0f;
    private float m_CurrentBossSpawnTime = 0f;
    private bool m_IsSpawning = true;

    private void Start()
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
        if (!(IsServer && m_IsSpawning))
            return;

        UpdateEnemySpawning();

        UpdateMeteorSpawning();

        UpdateBossSpawning();
    }

    private void UpdateEnemySpawning()
    {
        m_CurrentEnemySpawnTime += Time.deltaTime;
        if (m_CurrentEnemySpawnTime >= m_EnemySpawnTime.Value)
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

    private void UpdateBossSpawning()
    {
        m_CurrentBossSpawnTime += Time.deltaTime;
        if (m_CurrentBossSpawnTime >= m_bossSpawnTime.Value)
        {
            m_IsSpawning = false;
            StartCoroutine(BossAppear());
        }
    }

    IEnumerator BossAppear()
    {
        // Warning title and sound
        PlayWarnnigClientRpc();
        m_warningUI.SetActive(true);
        AudioManager.Instance.PlaySoundEffect(m_warningClip);

        // Same time as audio length
        yield return new WaitForSeconds(m_warningClip.length);

        StopWarnnigClientRpc();
        m_warningUI.SetActive(false);

        GameObject boss = NetworkObjectSpawner.SpawnNewNetworkObject(
            m_bossPrefabToSpawn,
            transform.position);

        BossController bossController = boss.GetComponent<BossController>();
        bossController.StartBoss(m_bossPosition.position);
        bossController.SetUI(m_bossUI);
        boss.name = "BOSS";
    }

    [ClientRpc]
    void PlayWarnnigClientRpc()
    {
        if (IsServer)
            return;

        m_warningUI.SetActive(true);

        AudioManager.Instance.PlaySoundEffect(m_warningClip);
    }

    [ClientRpc]
    void StopWarnnigClientRpc()
    {
        if (IsServer)
            return;

        m_warningUI.SetActive(false);
    }
}