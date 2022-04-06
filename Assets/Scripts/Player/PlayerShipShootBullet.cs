using UnityEngine;
using Unity.Netcode;

public class PlayerShipShootBullet : NetworkBehaviour
{
    [SerializeField]
    int m_fireDamage;

    [SerializeField]
    GameObject m_bulletPrefab;

    [SerializeField]
    Transform m_cannonPosition;

    [SerializeField]
    CharacterDataSO m_characterData;

    [SerializeField]
    GameObject m_shootVfx;

    [SerializeField]
    AudioClip m_shootClip;

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Tell the server to spawn the bullet
                FireServerRpc();
            }
        }
    }

    [ServerRpc]
    void FireServerRpc()
    {
        // Spawn the bullet and set the damage and character for stats
        NetworkObjectSpawner.SpawnNewNetworkObject(
            m_shootVfx,
            m_cannonPosition.position,
            Quaternion.identity);

        GameObject newBullet = NetworkObjectSpawner.SpawnNewNetworkObject(
            m_bulletPrefab,
            m_cannonPosition.position,
            Quaternion.identity);

        BulletController bulletController = newBullet.GetComponent<BulletController>();
        bulletController.damage = m_fireDamage;
        bulletController.characterData = m_characterData;

        // Tell the clients to reproduce the shoot sound
        PlayShootBulletSoundClientRpc();
    }

    [ClientRpc]
    void PlayShootBulletSoundClientRpc()
    {
        // Reproduce the ssfx on shoot on all clients
        AudioManager.Instance?.PlaySound(m_shootClip);
    }
}
