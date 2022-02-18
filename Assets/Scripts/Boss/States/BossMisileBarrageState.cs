using System.Collections;
using UnityEngine;

public class BossMisileBarrageState : BaseBossState
{
    [SerializeField]
    Transform[] _misileSpawningArea;
    [SerializeField]
    GameObject _misilePrefab;    
    [SerializeField]
    [Range(0f, 1f)]
    float misileDelayBetweenSpawns;
    
    IEnumerator RunMisileBarrageState()
    {
        // Spawn the missiles
        foreach (Transform spawnPosition in _misileSpawningArea)
        {
            FireMisiles(spawnPosition.position);
            yield return new WaitForSeconds(misileDelayBetweenSpawns);
        }

        // Go idle from a moment                
        m_controller.SetState(BossState.idle);
    }

    // Spawn the missile prefab
    void FireMisiles(Vector3 position)
    {
        NetworkSpawnController.SpawnHelper(_misilePrefab, position, _misilePrefab.transform.rotation);
    }

    // Run state
    public override void RunState()
    {
        StartCoroutine(RunMisileBarrageState());
    }
}
