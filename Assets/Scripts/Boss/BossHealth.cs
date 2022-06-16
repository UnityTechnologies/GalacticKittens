using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class BossHealth : NetworkBehaviour, IDamagable
{
    public int Health => m_health.Value;

    [Header("Health is multiplied by BaseHealth for the number of clients")]
    [Min(1)]
    [SerializeField]
    private int m_baseHealth = 15;
    
    private readonly NetworkVariable<int> m_health = new NetworkVariable<int>();

    [SerializeField]
    private SpriteRenderer[] m_sprites;

    [SerializeField]
    [Range(0f, 1f)]
    private float m_hitEffectDuration;

    [SerializeField]
    private Animator m_animator;

    [SerializeField]
    private BossController m_bossController;

    private bool m_isInmmune;

    private const string k_effectHit = "_Hit";
    private const string k_animHit = "hit";

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // check the number of players connected
            int playersConnected = NetworkManager.Singleton.ConnectedClientsList.Count;

            // The health is based on the number of clients for a better balance
            m_health.Value = playersConnected * m_baseHealth;
        }

        base.OnNetworkSpawn();
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

    [ClientRpc]
    private void HitEffectClientRpc()
    {
        StopCoroutine(HitEffect());
        StartCoroutine(HitEffect());
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
}
