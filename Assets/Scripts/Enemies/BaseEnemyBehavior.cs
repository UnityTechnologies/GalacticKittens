using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class BaseEnemyBehavior : NetworkBehaviour, IDamagable
{
    protected enum EnemyMovementType
    {
        linear,
        sineWave,

        // you can add more movement types here

        COUNT //MAX - used to get random value
    }

    protected enum EnemyState : byte
    {
        active,
        defeatAnimation,
        defeated
    }

    [SerializeField]
    protected float m_EnemySpeed = 4f;

    [SerializeField]
    protected NetworkVariable<float> m_EnemyLifetime =
        new NetworkVariable<float>(NetworkVariableReadPermission.Everyone, 20f);

    [SerializeField]
    protected bool m_UsesEnemyLifetime = true;

    [SerializeField]
    protected NetworkVariable<int> m_EnemyHealthPoints =
        new NetworkVariable<int>(NetworkVariableReadPermission.Everyone, 3);

    [SerializeField]
    protected GameObject m_VfxExplosion;

    protected NetworkVariable<EnemyState> m_EnemyState =
        new NetworkVariable<EnemyState>(NetworkVariableReadPermission.Everyone, EnemyState.active);

    protected EnemyMovementType m_EnemyMovementType;

    protected Vector2 m_Direction = Vector2.left;

    protected float m_WaveAmplitude;

    [SerializeField]
    SpriteRenderer m_sprite;
    [SerializeField]
    float m_hitEffectDuration;

    public override void OnNetworkSpawn()
    {
        m_WaveAmplitude = Random.Range(2f, 6f);

        base.OnNetworkSpawn();
    }

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

    protected virtual void UpdateActive()
    {
    }

    protected virtual void UpdateDefeatedAnimation()
    {
    }

    protected virtual void MoveEnemy()
    {
        if (m_EnemyMovementType == EnemyMovementType.sineWave)
        {
            m_Direction.x = -1f; //to move from right to left
            m_Direction.y = Mathf.Sin(Time.time * m_WaveAmplitude);

            m_Direction.Normalize();
        }

        // move the enemy in the desired direction
        transform.Translate(m_Direction * m_EnemySpeed * Time.deltaTime);
    }

    protected EnemyMovementType GetRandomEnemyMovementType()
    {
        int randomValue = Random.Range(0, (int)EnemyMovementType.COUNT);

        return (EnemyMovementType)randomValue;
    }

    protected void DespawnEnemy()
    {
        gameObject.SetActive(false);

        // Server tells clients that this object is no longer in play
        if (NetworkObject != null && NetworkObject.IsSpawned) // <-- safety check
            NetworkObject.Despawn();

        Destroy(gameObject);
    }


    public virtual void Hit(int damage)
    {
        if (!IsServer) return;
        m_EnemyHealthPoints.Value -= 1;
        StopCoroutine(HitEffect());
        StartCoroutine(HitEffect());
    }

    public IEnumerator HitEffect()
    {
        bool active = false;
        float timer = 0;
        while (timer < m_hitEffectDuration)
        {
            active = !active;
            m_sprite.material.SetInt("_Hit", active ? 1 : 0);
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }

        m_sprite.material.SetInt("_Hit", 0);
    }
}