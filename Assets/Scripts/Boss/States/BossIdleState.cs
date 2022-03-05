using System.Collections;
using UnityEngine;

public class BossIdleState : BaseBossState
{
    [SerializeField]
    [Range(0.1f, 2f)]
    float m_idleTime;

    IEnumerator RunIdleState()
    {
        // Wait for a moment
        yield return new WaitForSeconds(m_idleTime);

        // Call the fire state
        m_controller.SetState(BossState.fire);
    }

    public override void RunState()
    {
        StartCoroutine(RunIdleState());
    }
}
