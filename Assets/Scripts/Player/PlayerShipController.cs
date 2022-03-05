using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerShipController : NetworkBehaviour, IDamagable
{
    public NetworkVariable<int> health = new NetworkVariable<int>();

    [SerializeField]
    int m_fireDamage;

    [SerializeField]
    int m_maxSpecialPower;

    [SerializeField]
    GameObject m_bulletPrefab;

    [SerializeField]
    GameObject m_defenseMatrix;

    [SerializeField]
    Transform m_cannonPosition;

    [SerializeField]
    CharacterDataSO m_characterData;    

    [SerializeField]
    GameObject m_explosionVfxPrefab;

    [SerializeField]
    GameObject m_shootVfx;

    [SerializeField]
    float m_hitEffectDuration;

    [Header("AudioClips")]
    [SerializeField]
    AudioClip m_hitClip;

    [SerializeField]
    AudioClip m_shieldClip;

    [SerializeField]
    AudioClip m_shootClip;

    [Header("ShipSprites")]
    [SerializeField]
    SpriteRenderer m_shipRenderer;

    [Header("Runtime set")]
    public PlayerUI playerUI;

    public CharacterDataSO characterData;

    public GameplayManager gameplayManager;

    NetworkVariable<int> m_specials = new NetworkVariable<int>(0);

    bool m_death;

    const string k_hitEffect = "_Hit";

    void Update()
    {
        if (IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Tell the server to spawn the bullet
                FireServerRpc();
            }

            if (Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.LeftShift))
            {
                // Tell the server to activate the shield
                FireSpecialServerRpc();
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
    void FireServerRpc()
    {
        // Spawn the bullet and set the damage and character for stats
        NetworkSpawnController.SpawnHelper(m_shootVfx, m_cannonPosition.position);

        GameObject bullet = NetworkSpawnController.SpawnHelper(m_bulletPrefab, m_cannonPosition.position);
        BulletController bulletController = bullet.GetComponent<BulletController>();
        bulletController.damage = m_fireDamage;
        bulletController.characterData = m_characterData;

        // Tell the clients to reproduce the shoot sound
        PlayShootBulletSoundClientRpc();
    }

    [ServerRpc]
    void FireSpecialServerRpc()
    {
        // Activate the special in case the ship has available
        if (m_specials.Value > 0)
        {
            // Tell the UI to remove the icon
            playerUI.UpdatePowerUp(m_specials.Value, false);

            // Update the UI on clients, reduce the number of specials available
            m_specials.Value--;

            // Activate the special on clients for sync
            SpecialActivateClientRpc();

            // Update the power up use for the final score
            characterData.powerUpsUsed++;
        }
    }

    [ClientRpc]
    void SpecialActivateClientRpc()
    {
        // Activate the special
        m_defenseMatrix.SetActive(true);
        AudioManager.Instance?.PlaySound(m_shieldClip);
    }

    [ClientRpc]
    void PlayShootBulletSoundClientRpc()
    {
        // Reproduce the ssfx on shoot on all clients
        AudioManager.Instance?.PlaySound(m_shootClip);
    }

    [ClientRpc]
    void PlayShipHitSoundClientRpc(ulong clientId)
    {
        // Reproduce the sfx hit only on the client instance
        if (NetworkObject.OwnerClientId == clientId)
            AudioManager.Instance?.PlaySound(m_hitClip);
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
        LoadingSceneManager.Instance.LoadScene(SceneName.Menu, false);
    }

    [ClientRpc]
    void ShutdownClientRpc()
    {
        if (IsServer)
            return;

        Shutdown();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (IsServer)
        {
            // If the collider hit a power-up
            if (collider.TryGetComponent(out PowerUpSpecial powerUp))
            {
                // Check if i have space to take the special
                if (m_specials.Value < m_maxSpecialPower)
                {
                    // Update var
                    m_specials.Value++;

                    // Update UI
                    playerUI.UpdatePowerUp(m_specials.Value, true);

                    // Remove the power-up
                    powerUp.Despawn();
                }
            }
        }
    }

    // Sync the hit effect to all clients
    [ClientRpc]
    void HitClientRpc()
    {
        // Hit effect sync
        StopCoroutine(HitEffect());
        StartCoroutine(HitEffect());
    }

    public void Hit(int damage)
    {
        if (!IsServer || m_death)
            return;

        // Update health var
        health.Value -= damage;

        // Update UI
        playerUI.UpdateHealth(health.Value);

        // Sync on client
        HitClientRpc();

        if (health.Value > 0)
        {
            PlayShipHitSoundClientRpc(NetworkObject.OwnerClientId);
        }
        else if (health.Value <= 0)
        {
            // When death set the bool so this is only call one time
            m_death = true;

            // Spawn the death vfx
            NetworkSpawnController.SpawnHelper(m_explosionVfxPrefab, this.transform.position);

            // Tell the Gameplay manager that I've been defeated
            gameplayManager.PlayerDeath(m_characterData.clientId);

            // Safety check
            if (NetworkObject != null || NetworkObject.IsSpawned)
                NetworkObject.Despawn();
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