# Basic Enemies

There are 3 basic enemies that appear in the gameplay scene:

<table align="center" style="text-align:center;" width="100%">
  <tr>
    <th>
      <img src="..\Images\enemy_E1.png"/>
    </th>
    <th>
      <img src="..\Images\enemy_E2.png"/>
    </th>
    <th>
      <img src="..\Images\meteorite_03.png"/>
    </th>
  </tr>
  <tr>
    <th>
      <p>Shooter Shooter</p>
    </th>
    <th>
      <p>Space Ghost</p>
    </th>
    <th>
      <p>Meteorite</p>
    </th>
  </tr>
</table>

<br>

All of these enemies are spawned and owned by the `Host or Server` of the network session:

* `Space Shooter Enemy`: Randomly shoots a bullet that can hurt the player spaceships, but can be defeated with just 1 hit.

* `Space Ghost Enemy`: The heavier enemy type, which needs 3 shots in order to be defeated.

* `Meteorite`: These seemingly harmless obstacles can hurt the player spaceships, and are spawned in different sizes!

<br>

### Enemy Base Class

The `Space Shooter` and the `Space Ghost` inherit from the `BaseEnemyBehavior` base class.


#### Enemy Movement
A movement pattern gets assigned in the enemy creation. These movement types can be declared in this enum located in the base class. The movement gets randomly selected when the enemy is spawned:
```C#
public class BaseEnemyBehavior : NetworkBehaviour, IDamagable
{
    protected enum EnemyMovementType
    {
        linear,
        sineWave,

        // you can add more movement types here

        COUNT //MAX - used to get random value
    }

    ...

    public override void OnNetworkSpawn()
    {
        m_EnemyMovementType = GetRandomEnemyMovementType();

        base.OnNetworkSpawn();
    }

    ...
}
```
* `linear`: The enemy will move from left to right at constant speed, at a constant -X direction.
* `sineWave`: The enemy will move from left to right, but following a sine wave pattern with a random wave amplitude.


#### Enemy States
The Space Shooter and Space Ghost run their own small states:
```C#
    protected enum EnemyState : byte
    {
        active,
        defeatAnimation,
        defeated
    }
```

These states are then used on the `Update` function of the base class:
```C#
    protected virtual void Update()
    {
        if (m_EnemyState.Value == EnemyState.active)
        {
            UpdateActive();
        }
        else if (m_EnemyState.Value == EnemyState.defeatAnimation)
        {
            UpdateDefeatedAnimation();
        }
        else // (m_EnemyState.Value == EnemyState.defeated)
        {
            DespawnEnemy();
        }
    }
```
If you'd like, you can add more states and functions to this update, to create your own additional states and behaviors!