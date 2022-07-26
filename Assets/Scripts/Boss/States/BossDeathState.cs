using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BossDeathState : BaseBossState
{
    [SerializeField]
    int m_maxNumberOfExplosions;

    [SerializeField]
    float m_explosionDuration;

    [SerializeField]
    Transform m_explosionPositionsContainer;

    [SerializeField]
    GameObject m_explosionVfx;

    [SerializeField]
    [Range(1f, 40f)]
    float m_shakeSpeed;

    [SerializeField]
    [Range(0.1f, 2f)]
    float m_shakeAmount;

    List<Transform> explosionPositions = new List<Transform>();

    void Start()
    {
        if (IsServer)
        {
            // Add the explosions Positions 
            foreach (Transform transform in m_explosionPositionsContainer)
            {
                explosionPositions.Add(transform);
            }
        }
    }

    IEnumerator Shake()
    {
        float currentPositionx = transform.position.x;
        while (true)
        {
            float shakeValue = Mathf.Sin(Time.time * m_shakeSpeed) * m_shakeAmount;

            transform.position = new Vector2(currentPositionx + shakeValue, transform.position.y);

            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator RunDeath()
    {
        // Show various explosion vfx for some seconds
        int numberOfExplosions = 0;
        float stepDuration = m_explosionDuration / m_maxNumberOfExplosions;

        StartCoroutine(Shake());
        while (numberOfExplosions < m_maxNumberOfExplosions)
        {
            Vector3 randPosition = explosionPositions[Random.Range(0, explosionPositions.Count)].position;

            NetworkObjectSpawner.SpawnNewNetworkObject(m_explosionVfx, randPosition);

            yield return new WaitForSeconds(stepDuration);

            numberOfExplosions++;
        }
        StopCoroutine(Shake());

        yield return new WaitForEndOfFrame();
        GameplayManager.Instance.BossDefeat();

        NetworkObjectDespawner.DespawnNetworkObject(NetworkObject);
    }

    public override void RunState()
    {
        StartCoroutine(RunDeath());
    }
}