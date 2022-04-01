using System.Collections;
using UnityEngine;

public class BossMisileBarrageState : BaseBossState
{
    [SerializeField]
    Transform[] m_misileSpawningArea;

    [SerializeField]
    GameObject m_misilePrefab;

    [SerializeField]
    [Range(0f, 1f)]
    float m_misileDelayBetweenSpawns;
    
    IEnumerator RunMisileBarrageState()
    {
        // Spawn the missiles
        foreach (Transform spawnPosition in m_misileSpawningArea)
        {
            FireMisiles(spawnPosition.position);
            yield return new WaitForSeconds(m_misileDelayBetweenSpawns);
        }

        // Go idle from a moment
        m_controller.SetState(BossState.idle);
    }

    // Spawn the missile prefab
    void FireMisiles(Vector3 position)
    {
        NetworkObjectSpawner.SpawnNewNetworkObject(
            m_misilePrefab,
            position,
            m_misilePrefab.transform.rotation);
    }

    // Run state
    public override void RunState()
    {
        StartCoroutine(RunMisileBarrageState());
    }
}
