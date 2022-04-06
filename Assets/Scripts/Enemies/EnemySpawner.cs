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
        new NetworkVariable<float>(NetworkVariableReadPermission.Everyone, 5f);

    [SerializeField]
    private NetworkVariable<float> _bossSpawnTime =
        new NetworkVariable<float>(NetworkVariableReadPermission.Everyone, 10f);

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

    private float m_CurrentSpawnTime = 0f;
    private float _currentBossSpawnTime = 0f;
    private bool isSpawning = true;
    private float m_meteorTimer = 0f;

    // Update is called once per frame
    void Update()
    {
        if (IsServer && isSpawning)
        {
            m_CurrentSpawnTime += Time.deltaTime;
            _currentBossSpawnTime += Time.deltaTime;
            if (m_CurrentSpawnTime >= m_EnemySpawnTime.Value)
            {
                Vector3 position = GetNextRandomEnemyPrefabPosition();

                var nextPrefabToSpawn = GetNextRandomEnemyPrefabToSpawn();

                var newEnemy = NetworkObjectSpawner.SpawnNewNetworkObject(nextPrefabToSpawn);
                newEnemy.transform.position = position;

                m_CurrentSpawnTime = 0f;
            }

            m_meteorTimer += Time.deltaTime;
            if (m_meteorTimer > m_meteorSpawningTime)
            {
                m_meteorTimer = 0f;
                SpawnMeteor();
            }

            if (_currentBossSpawnTime >= _bossSpawnTime.Value)
            {
                isSpawning = false;
                StartCoroutine(BossAppear());
            }
        }
    }

    void SpawnMeteor()
    {
        // The min and max Y pos for spawning the meteors
        float randomYpos = Random.Range(-5f, 6f);
        Vector3 meteorSpawnPosition = new Vector3(transform.position.x, randomYpos, 0f);

        NetworkObjectSpawner.SpawnNewNetworkObject(m_meteorPrefab, meteorSpawnPosition, Quaternion.identity);
    }

    IEnumerator BossAppear()
    {
        // Warning title and sound
        PlayWarnnigClientRpc();
        m_warningUI.SetActive(true);
        AudioManager.Instance.PlaySound(m_warningClip);

        // Same time as audio length
        yield return new WaitForSeconds(m_warningClip.length);

        StopWarnnigClientRpc();
        m_warningUI.SetActive(false);

        GameObject boss = NetworkObjectSpawner.SpawnNewNetworkObject(
            m_bossPrefabToSpawn,
            transform.position,
            Quaternion.identity);

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

        AudioManager.Instance.PlaySound(m_warningClip);
    }

    [ClientRpc]
    void StopWarnnigClientRpc()
    {
        if (IsServer)
            return;

        m_warningUI.SetActive(false);
    }

    [ClientRpc]
    void RemoveWarningClientRpc()
    {
        if (IsServer)
            return;

        m_warningUI.SetActive(false);
    }

    Vector3 GetNextRandomEnemyPrefabPosition()
    {
        float randomYvalue = Random.Range(-5f, 5f);

        return new Vector3(transform.position.x, randomYvalue, 0f);
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
}