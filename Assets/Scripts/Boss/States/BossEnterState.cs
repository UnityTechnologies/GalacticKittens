using System.Collections;
using UnityEngine;

public class BossEnterState : BaseBossState
{
    [SerializeField]
    private float m_speed;

    [HideInInspector]
    [Header("Set in runtime")]
    public Vector3 initialPosition;

    // Run the state
    public override void RunState()
    {
        StartCoroutine(RunEnterState());
    }

    IEnumerator RunEnterState()
    {
        // While the boss is far for the initial pos, the boss moves close with a curve
        while (Vector2.Distance(transform.position, initialPosition) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                initialPosition,
                m_speed * Time.deltaTime);

            transform.position = new Vector2(
                transform.position.x,
                transform.position.y + (Mathf.Sin(Time.time) * 0.01f));

            yield return new WaitForEndOfFrame();
        }

        // When the boss finish the enter movement start with the fire state
        m_controller.SetState(BossState.fire);
    }
}
