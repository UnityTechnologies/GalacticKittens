# Boss Battle

The boss is made of various scripts that inheritsfrom BaseBossState:

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

`BossEnterState`: Script that controls how the boss enters the field. The developer can modify this behavior or create a new one entirely.