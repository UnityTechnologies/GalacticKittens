using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class HomingMisile : NetworkBehaviour
{
    [SerializeField]
    int m_damage = 1;

    [SerializeField]
    float m_startingSpeed = 4f;

    [SerializeField]
    float m_followSpeed = 8f;

    [SerializeField]
    float m_startingTime = 0.5f;

    [SerializeField]
    float m_followTime = 2f;

    [Header("Set in runtime")]
    [HideInInspector]
    [SerializeField]
    Transform m_targetToHit;

    private IEnumerator MisileHoming()
    {
        float timer = 0f;

        // Important: the axis we are using for the direction of move is the positive X, take this into account were using another prefab

        // Starting -> Going up.
        while (true)
        {
            yield return new WaitForEndOfFrame();
            transform.Translate(Vector2.right * m_startingSpeed * Time.deltaTime);

            timer += Time.deltaTime;
            if (timer > m_startingTime)
            {
                break;
            }
        }

        timer = 0f;

        // Following -> Move towards the target
        while (true)
        {
            yield return new WaitForEndOfFrame();

            // Safety check because maybe the target dies before i hit
            if (m_targetToHit != null)
            {
                Vector2 dir = m_targetToHit.position - transform.position;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                transform.position = Vector2.MoveTowards(transform.position, m_targetToHit.position, Time.deltaTime * m_followSpeed);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, 0f, angle), Time.deltaTime * 5f);
            }
            else
            {
                break;
            }

            timer += Time.deltaTime;
            if (timer > m_followTime)
            {
                break;
            }
        }

        // Breaking -> stop following the target and just continue on the same direction
        while (true)
        {
            yield return new WaitForEndOfFrame();
            transform.Translate(Vector2.right * m_followSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (IsServer)
        {
            if (collider.TryGetComponent(out IDamagable damagable))
            {
                collider.GetComponent<IDamagable>().Hit(m_damage);
                StopAllCoroutines();
                
                NetworkObjectDespawner.DespawnNetworkObject(NetworkObject);
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Select a player to follow
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            m_targetToHit = players[Random.Range(0, players.Length)].transform;

            // Start misile routine
            StartCoroutine(MisileHoming());

            base.OnNetworkSpawn();
        }
    }
}
