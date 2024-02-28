using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerShipController : NetworkBehaviour, IDamagable
{
    public bool DoesTakeDamage = false;
    public NetworkVariable<int> health = new NetworkVariable<int>();

    [SerializeField]
    int m_maxSpecialPower;

    [SerializeField]
    DefenseMatrix m_defenseShield;

    [SerializeField]
    GameObject m_explosionVfxPrefab;

    [SerializeField]
    float m_hitEffectDuration;

    [Header("AudioClips")]
    [SerializeField]
    AudioClip m_hitClip;

    [SerializeField]
    AudioClip m_shieldClip;

    [Header("ShipSprites")]
    [SerializeField]
    SpriteRenderer m_shipRenderer;

    NetworkVariable<int> m_specials = new NetworkVariable<int>(0);

    bool m_isPlayerDefeated;

    const string k_hitEffect = "_Hit";

    void Update()
    {
        if (IsOwner)
        {
            if (DoesTakeDamage && 
                !m_defenseShield.isShieldActive &&
                (Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.LeftShift)))
            {
                // Tell the server to activate the shield
                ActivateShieldServerRpc();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Exit the network state and return to the menu
                if (IsServer) // Host
                {
                    // All player should shutdown and exit
                    StartCoroutine(HostShutdown());
                }
                else
                {
                    Shutdown();
                }
            }
        }
    }

    [ServerRpc]
    void ActivateShieldServerRpc()
    {
        // Activate the special in case the ship has available
        if (m_specials.Value > 0)
        {
            // Update the UI on clients, reduce the number of specials available
            m_specials.Value--;

            // Activate the special on clients for sync
            ActivateShieldClientRpc();
        }
    }

    [ClientRpc]
    void ActivateShieldClientRpc()
    {
        // Activate the shield
        m_defenseShield.TurnOnShield();

        AudioManager.Instance?.PlaySoundEffect(m_shieldClip);
    }

    [ClientRpc]
    void PlayShipHitSoundClientRpc(ulong clientId)
    {
        // Reproduce the sfx hit only on the client instance
        if (NetworkObject.OwnerClientId == clientId)
            AudioManager.Instance?.PlaySoundEffect(m_hitClip);
    }

    IEnumerator HostShutdown()
    {
        // Tell the clients to shutdown
        ShutdownClientRpc();

        // Wait some time for the message to get to clients
        yield return new WaitForSeconds(0.5f);

        // Shutdown server/host
        Shutdown();
    }

    // Shutdown the network session and load the menu scene
    void Shutdown()
    {
        NetworkManager.Singleton.Shutdown();
    }

    [ClientRpc]
    void ShutdownClientRpc()
    {
        if (IsServer)
            return;

        Shutdown();
    }

    // Sync the hit effect to all clients
    [ClientRpc]
    void PlayHitEffectClientRpc()
    {
        // Hit effect sync
        StopCoroutine(HitEffect());
        StartCoroutine(HitEffect());
    }

    public void Hit(int damage)
    {
        if (!IsServer || m_isPlayerDefeated)
            return;

        // Update health var
        if(DoesTakeDamage)
            health.Value -= damage;

        // Sync on client
        PlayHitEffectClientRpc();

        if (health.Value > 0)
        {
            PlayShipHitSoundClientRpc(NetworkObject.OwnerClientId);
        }
        else // (health.Value <= 0)
        {
            // When death set the bool so this is only call one time
            m_isPlayerDefeated = true;

            // Spawn the death vfx
            NetworkObjectSpawner.SpawnNewNetworkObject(
                m_explosionVfxPrefab,
                transform.position,
                Quaternion.identity);
            
            NetworkObjectDespawner.DespawnNetworkObject(NetworkObject);
        }
    }

    // Set the hit animation effect
    public IEnumerator HitEffect()
    {
        bool active = false;
        float timer = 0f;

        while (timer < m_hitEffectDuration)
        {
            active = !active;
            m_shipRenderer.material.SetInt(k_hitEffect, active ? 1 : 0);
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }

        m_shipRenderer.material.SetInt(k_hitEffect, 0);
    }
}