using System.Collections;
using UnityEngine;

public class BossFireState : BaseBossState
{
    [SerializeField]
    Transform[] _fireCannonSpawningArea;

    [SerializeField]
    GameObject _trioBulletPrefab;

    [SerializeField]
    GameObject _circularBulletPrefab;

    [SerializeField]
    float _normalShootRateOfFire;

    [SerializeField]
    float _idleSpeed;

    IEnumerator FireState()
    {
        // Setup initial vars
        float shootTimer = 0f;
        float normalStateTimer = 0f;
        float normalStateExitTime = Random.Range(7f, 21f);

        // We have a random time on this state so while we are in this state we proceed to fire
        while (normalStateTimer <= normalStateExitTime)
        {
            // Small movement on the boss
            transform.position = new Vector2(transform.position.x, Mathf.Sin(Time.time) * _idleSpeed);

            // every x time shoot (trio or circular)
            shootTimer += Time.deltaTime;
            if (shootTimer >= _normalShootRateOfFire)
            {
                // trio -> 7/10, circular -> 3/10
                int randomFire = Random.Range(0, 10);

                // print($"RandomFire:: {randomFire}");
                Fire(randomFire < 7 ? _trioBulletPrefab : _circularBulletPrefab);

                shootTimer = 0;
            }

            yield return new WaitForEndOfFrame();

            normalStateTimer += Time.deltaTime;
        }
        
        // When we end the time on this state call the special attack, it can be a different state or 
        // a random for different states
        m_controller.SetState(BossState.misileBarrage);
    }


    void Fire(GameObject firePrefab)
    {
        // Because the cannon positions are lower on the sprite with increase the rotation up 
        float randomRotation = Random.Range(-25f, 45f);
        foreach (Transform laseCannon in _fireCannonSpawningArea)
        {
            NetworkObjectSpawner.SpawnNewNetworkObject(
                firePrefab,
                laseCannon.position,
                Quaternion.Euler(0f, 0f, randomRotation)
            );
        }
    }

    public override void RunState()
    {
        StartCoroutine(FireState());
    }
}
