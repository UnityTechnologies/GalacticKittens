using System.Collections;
using UnityEngine;

public class BossFireState : BaseBossState
{
    [SerializeField]
    private Transform[] _fireCannonSpawningArea;

    [SerializeField]
    private GameObject _trioBulletPrefab;

    [SerializeField]
    private GameObject _circularBulletPrefab;

    [SerializeField]
    private float _normalShootRateOfFire;

    [SerializeField]
    private float _idleSpeed;

    public override void RunState()
    {
        StartCoroutine(FireState());
    }

    private IEnumerator FireState()
    {
        // Setup initial vars
        float shootTimer = 0f;
        float normalStateTimer = 0f;
        float normalStateExitTime = Random.Range(7f, 21f);

        // We have a random time on this state so while we are in this state we proceed to fire
        while (normalStateTimer <= normalStateExitTime)
        {
            // Small movement on the boss
            transform.position = new Vector2(
                transform.position.x,
                Mathf.Sin(Time.time) * _idleSpeed);

            // every x time shoot (trio or circular)
            shootTimer += Time.deltaTime;
            if (shootTimer >= _normalShootRateOfFire)
            {
                GameObject nextBulletPrefabToShoot = GetNextBulletPrefabToShoot();

                FireBulletPrefab(nextBulletPrefabToShoot);

                shootTimer = 0f;
            }

            yield return new WaitForEndOfFrame();

            normalStateTimer += Time.deltaTime;
        }
        
        // When we end the time on this state call the special attack, it can be a different state
        // or a random for different states
        m_controller.SetState(BossState.misileBarrage);
    }

    private GameObject GetNextBulletPrefabToShoot()
    {
        int randomBulletChoice = Random.Range(0, 10);

        // trio -> 7/10, circular -> 3/10
        if (randomBulletChoice < 7)
        {
            return _trioBulletPrefab;
        }

        return _circularBulletPrefab;
    }

    private void FireBulletPrefab(GameObject bulletPrefab)
    {
        // Because the cannon positions are lower on the sprite with increase the rotation up
        float randomZrotation = Random.Range(-25f, 45f);

        foreach (Transform laserCannon in _fireCannonSpawningArea)
        {
            NetworkObjectSpawner.SpawnNewNetworkObject(
                bulletPrefab,
                laserCannon.position,
                Quaternion.Euler(0f, 0f, randomZrotation)
            );
        }
    }
}
