using System.Collections;
using UnityEngine;

public class BossEnterState : BaseBossState
{
    [SerializeField]
    float m_speed;

    [Header("Set in runtime")]
    public Vector3 initialPosition;

    // Run the state
    public override void RunState()
    {
        StartCoroutine(RunEnterState());
    }    

    IEnumerator RunEnterState()
    {
        // While the boss is far for the initial pos, the boss move close with a curve
        while (Vector2.Distance(transform.position, initialPosition) > 0.01f)
        {            
            transform.position = Vector2.MoveTowards(transform.position, initialPosition, m_speed * Time.deltaTime);
            transform.position = new Vector2(transform.position.x, transform.position.y + (Mathf.Sin(Time.time) * 0.01f));

            yield return new WaitForEndOfFrame();
        }

        // When the boss finish the enter movement start with the fire state
        m_controller.SetState(BossState.fire);
    }
}
