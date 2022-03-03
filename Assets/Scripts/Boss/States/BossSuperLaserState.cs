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
        float randomRotation = UnityEngine.Random.Range(-40f, 10f);
        GameObject superLaser = NetworkSpawnController.SpawnHelper(m_superLaserPrefab, m_superLaserPosition.localPosition, Quaternion.Euler(0, 0, randomRotation));                
        // TODO: Wait the time the vfx last
        yield return new WaitForSeconds(5);
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