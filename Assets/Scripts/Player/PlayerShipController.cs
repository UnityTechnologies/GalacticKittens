using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerShipController : NetworkBehaviour, IDamagable
{
    public NetworkVariable<int> health = new NetworkVariable<int>();

    [SerializeField]
    int m_maxSpecialPower;

    [SerializeField]
    DefenseMatrix m_defenseShield;

    [SerializeField]
    CharacterDataSO m_characterData;

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

    [Header("Runtime set")]
    [HideInInspector]
    public PlayerUI playerUI;

    [HideInInspector]
    public CharacterDataSO characterData;

    [HideInInspector]
    public GameplayManager gameplayManager;

    NetworkVariable<int> m_specials = new NetworkVariable<int>(0);

    bool m_isPlayerDefeated;

    const string k_hitEffect = "_Hit";

    void Update()
    {
        if (IsOwner)
        {
            if (!m_defenseShield.isShieldActive &&
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
            // Tell the UI to remove the icon
            playerUI.UpdatePowerUp(m_specials.Value, false);

            // Update the UI on clients, reduce the number of specials available
            m_specials.Value--;

            // Activate the special on clients for sync
            ActivateShieldClientRpc();

            // Update the power up use for the final score
            characterData.powerUpsUsed++;
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
        if (!IsServer)
            return;
        
        // If the collider hit a power-up
        if (collider.gameObject.CompareTag("PowerUpSpecial"))
        {
            // Check if I have space to take the special
            if (m_specials.Value < m_maxSpecialPower)
            {
                // Update var
                m_specials.Value++;

                // Update UI
                playerUI.UpdatePowerUp(m_specials.Value, true);

                // Remove the power-up
                NetworkObjectDespawner.DespawnNetworkObject(
                    collider.gameObject.GetComponent<NetworkObject>());
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
        if (!IsServer || m_isPlayerDefeated)
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
        else // (health.Value <= 0)
        {
            // When death set the bool so this is only call one time
            m_isPlayerDefeated = true;

            // Spawn the death vfx
            NetworkObjectSpawner.SpawnNewNetworkObject(
                m_explosionVfxPrefab,
                transform.position,
                Quaternion.identity);

            // Tell the Gameplay manager that I've been defeated
            gameplayManager.PlayerDeath(m_characterData.clientId);
            
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