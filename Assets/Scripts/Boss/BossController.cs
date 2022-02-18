using Unity.Netcode;
using UnityEngine;

/*
    Script that controls how the boss is going to work,
    the diferent behaviours are set on diferent scripts. 
    Here you can add new states
*/

public class BossController : NetworkBehaviour
{
    [SerializeField]
    int m_damage;
    [Header("States for the boss")]
    [SerializeField]
    BossEnterState m_enterState;
    [SerializeField]
    BaseBossState m_fireState;    
    [SerializeField]
    BaseBossState m_misileBarrageState;
    [SerializeField]
    BaseBossState m_idleState;
    [SerializeField]
    BaseBossState m_deathState;
    [Header("For testing the boss states -> false for production")]
    [SerializeField]
    bool m_isTesting;
    [SerializeField]
    BossState m_testState;

    BossUI bossUI;

    void OnTriggerEnter2D(Collider2D collider)
    {
        // When the players get close to me do some damage
        if (collider.TryGetComponent(out PlayerShipController playerShip))
        {
            playerShip.Hit(m_damage);
        }
    }    

    // When hit update the UI
    public void OnHit(int currentHealth)
    {
        bossUI.UpdateUI(currentHealth);
    }

    // This will set the starting state for the boss -> enter state
    public void StartBoss(Vector3 initialPositionForEnterState)
    {
        m_enterState.initialPosition = initialPositionForEnterState;
        SetState(BossState.enter);
    }

    // Set the boss state to run
    // You can add more states to the bosss
    //..    
    public void SetState(BossState state)
    {
        if (IsServer)
        {
            switch (state)
            {
                case BossState.enter:
                    m_enterState.RunState();
                    break;
                case BossState.fire:
                    m_fireState.RunState();
                    break;
                case BossState.misileBarrage:
                    m_misileBarrageState.RunState();
                    break;                
                case BossState.idle:
                    m_idleState.RunState();
                    break;
                case BossState.death:
                    // Stop all coroutines from other state
                    // because the death can override any state                                        
                    m_enterState.StopState();
                    m_fireState.StopState();
                    m_misileBarrageState.StopState();                    
                    m_idleState.StopState();

                    m_deathState.RunState();
                    break;
            }
        }
    }

    // Set the boss UI
    public void SetUI(BossUI bossUI)
    {
        if (!IsServer) return;

        BossHealth bossHealth = GetComponentInChildren<BossHealth>();
        this.bossUI = bossUI;
        bossUI.SetHealth(bossHealth.Health);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // If you one to test the boss outside of the normal flow of the game
            if (m_isTesting)
                SetState(m_testState);
        }
        base.OnNetworkSpawn();
    }
}
