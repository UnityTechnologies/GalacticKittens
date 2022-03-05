using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class BossHealth : NetworkBehaviour, IDamagable
{    
    [Header("Health is multiply by BaseHealth for the number of clients")]
    [SerializeField] private int baseHealth;
    private NetworkVariable<int> m_health = new NetworkVariable<int>();

    public int Health
    {
        get { return m_health.Value; }
    }

    [SerializeField]
    SpriteRenderer[] m_sprites;

    [SerializeField]
    [Range(0f, 1f)]
    float m_hitEffectDuration;

    [SerializeField]
    Animator m_animator;

    [SerializeField]
    BossController m_bossController;

    bool m_isInmmune;

    const string k_effectHit = "_Hit";
    const string k_animHit = "hit";

    [ClientRpc]
    void HitEffectClientRpc()
    {
        StopCoroutine(HitEffect());
        StartCoroutine(HitEffect());
    }

    // For when someone hits me
    public void Hit(int damage)
    {
        if (!IsServer || m_isInmmune)
            return;

        m_health.Value -= damage;
        m_bossController.OnHit(m_health.Value);

        // Sync to clients
        HitEffectClientRpc();

        if (m_health.Value <= 0)
        {                      
            // If health is below or equal to 0 change to death state
            m_bossController.SetState(BossState.death);
        }
    }

    // The hit effect use in the game
    public IEnumerator HitEffect()
    {
        m_isInmmune = true;
        m_animator.SetBool(k_animHit, true);
        bool active = false;
        float timer = 0f;
        while (timer < m_hitEffectDuration)
        {
            active = !active;
            foreach (var sprite in m_sprites)
            {
                sprite.material.SetInt(k_effectHit, active ? 1 : 0);
            }
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }

        foreach (var sprite in m_sprites)
        {
            sprite.material.SetInt(k_effectHit, 0);
        }

        m_animator.SetBool(k_animHit, false);

        yield return new WaitForSeconds(0.2f);
        m_isInmmune = false;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // check the number of players connected
            int playersConnected = NetworkManager.Singleton.ConnectedClientsList.Count;

            // The health is based on the number of clients for a better balance
            m_health.Value = playersConnected * baseHealth;
        }

        base.OnNetworkSpawn();
    }
}
