using System.Collections;
using UnityEngine;

public class BossSuperLaserState : BaseBossState
{
    [SerializeField]
    GameObject m_superLaserPrefab;

    [SerializeField]
    Transform m_superLaserPosition;
    
    IEnumerator FireSuperLaser()
    {
        float randomRotation = Random.Range(-40f, 10f);

        GameObject superLaser = NetworkObjectSpawner.SpawnNewNetworkObject(
            m_superLaserPrefab,
            m_superLaserPosition.localPosition,
            Quaternion.Euler(0f, 0f, randomRotation)
        );

        // TODO: Wait the time the vfx last
        yield return new WaitForSeconds(5f);
        m_controller.SetState(BossState.idle);
    }
        
    public override void RunState()
    {
        StartCoroutine(FireSuperLaser());
    }

    public override void StopState()
    {
        StopCoroutine(FireSuperLaser());
    }  

}
