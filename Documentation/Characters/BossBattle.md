# Boss Battle

The boss is made of a state machine where every state defines either a pattern of attack, or the idle state. Every state is a script that inherits from the base class `BaseBossState`:

`BaseBossState:` This script should be the parent for all the states created for the boss.It is a simple base class that has two methods for overriding RunState() forrunning the state and StopState() for stopping all the coroutines, the last is used only when changing to death state because it can happen when the boss is already in another state:

```C#
[RequireComponent(typeof(BossController))]
public class BaseBossState : NetworkBehaviour
{
    protected BossController m_controller;

    private void Start()
    {
        m_controller = FindObjectOfType<BossController>();
    }
    
    // Method that should be run on all states
    public virtual void RunState() { }
    
    public virtual void StopState() 
    {
        StopAllCoroutines();
    }
}
```

It's easy to just create or modify the states. If you one to create a new way of attacking for the boss, that script will only take care of the new way of attacking.

These are the states of the boss:
```C#
public enum BossState
{ 
    fire,
    misileBarrage,
    death,
    idle,
    enter
};
```

<br>

`BossEnterState`: Here's an example of how one can create a new boss state. This script controls how the boss enters the scene.

```C#
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
```
Here are the other boss states used in the game:

`BossIdleState`: a middle state that just makes the boss idle for some time and the process to the fire state. It is used after the missile attack for some time, but it can be extended for other actions.

`BossFireState`: The main attack of the boss it controls when to fire and use two types of attacks. Also check a random value to change to the special attack Missile Barrage. The fire state can be modified on the editor to change the following:

* **FireCannonSpawningArea**: The transform of the area where the bullets would spawn
* **Trio bullet prefab**: the prefab for the type of attack with three bullets
* **Circular bullet prefab**: prefab for the circular attack
* **Normal shoot rate of fire**: Time for awaiting between shoots
* **Idle speed**: the idle speed of the boss when in this state.

`BossMissileBarrage`: Special attack that launches 4 missiles to track the players for a time and then continue straight.Missile barrage state can be modified on the editor to change the following:

* **Missile spawning area**: The transform position of the spawning areas for every missile, it can be less or more
* **Missile prefab**: the missie prefab
* **Missile delay between spawn**: delay between every missile spawn

`BossDeathState`: Death animation of the the boss. Displays various explosions while shaking the boss GameObject. This state can be modified on the editor to change the following:
* **Max number of explosions**: the number of explosions to spawn during the explosion effect.
* **Explosion duration**: how much time should the explosion effect last.
* **Explosion positions container**: The container for all the transforms where the explosions should appear.
* **Explosion vfx**: the vfx of explosion.
* **Shake speed**: the value of speed on the shake effect.
* **Shake Amount**: The amount of shake to make.

### Adding a New Boss State
If you'd like to add a new state, first create the a new script that inherits from the `BaseBossState` base class. Then, go into the `BossController` class to add the new state as a variable, and into the `SetState()` function as part of the switch statement:

```C#
public class BossController : NetworkBehaviour
{
    [SerializeField]
    private int m_damage;

    [SerializeField]
    private BossEnterState m_enterState;

    [SerializeField]
    private BaseBossState m_fireState;

    [SerializeField]
    private BaseBossState m_misileBarrageState;

    [SerializeField]
    private BaseBossState m_idleState;

    [SerializeField]
    private BaseBossState m_deathState;

    // your new state here...

    ...

    public void SetState(BossState state)
    {
        if (!IsServer)
            return;

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
                m_ewnterState.StopState();
                m_fireState.StopState();
                m_misileBarrageState.StopState();
                m_idleState.StopState();

                m_deathState.RunState();
                break;
        }
    }

    ...
}
```

