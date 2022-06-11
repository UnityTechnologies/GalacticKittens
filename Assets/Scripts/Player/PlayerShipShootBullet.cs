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
        if (!IsOwner)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            FireNewBulletServerRpc();
        }
    }

    [ServerRpc]
    void FireNewBulletServerRpc()
    {
        SpawnNewBulletVfx();

        GameObject newBullet = GetNewBullet();

        PrepareNewlySpawnedBulltet(newBullet);
        
        PlayShootBulletSoundClientRpc();
    }

    private void SpawnNewBulletVfx()
    {
        NetworkObjectSpawner.SpawnNewNetworkObject(m_shootVfx, m_cannonPosition.position);
    }

    private GameObject GetNewBullet()
    {
        return NetworkObjectSpawner.SpawnNewNetworkObject(
            m_bulletPrefab,
            m_cannonPosition.position);
    }

    private void PrepareNewlySpawnedBulltet(GameObject newBullet)
    {
        BulletController bulletController = newBullet.GetComponent<BulletController>();
        bulletController.damage = m_fireDamage;
        bulletController.characterData = m_characterData;
    }

    [ClientRpc]
    void PlayShootBulletSoundClientRpc()
    {
        AudioManager.Instance?.PlaySoundEffect(m_shootClip);
    }
}
