using System.Collections.Generic;
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

    [SerializeField]

    [Range(10, 50)]
    int MaximumPreSpawnedBullets = 20;

    private bool m_PlayerShoot;

    public Vector3 VisualOffset = new Vector3(0.5f, 0.0f, 0.0f);

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_PlayerShoot = true;
            //if (IsServer)
            //{
            //    FireNewBulletServerRpc(m_cannonPosition.position);
            //}
            //else
            //{

            //}

        }
    }

    [ServerRpc]
    void FireNewBulletServerRpc(Vector3 clientCannonPosition)
    {
        var startPosition = ((clientCannonPosition * 3) + m_NonAuthTargetPosition) * 0.25f + VisualOffset;
        m_ShootBullet = true;
        FireBullet(startPosition);
    }

    private void SpawnNewBulletVfx()
    {
        var objectSpawned = Instantiate(m_shootVfx);
        // Set the FX's local space positioin to the cannon's local space position;
        objectSpawned.transform.localPosition = m_cannonPosition.localPosition;
        NetworkObject newGameObjectNetworkObject = objectSpawned.GetComponent<NetworkObject>();
        newGameObjectNetworkObject.Spawn(false);
        // Parent the FX to the ship so it follows the ship
        newGameObjectNetworkObject.TrySetParent(GetComponent<NetworkObject>(), false);
    }

    private GameObject GetNewBullet(Vector3 position)
    {
        var objectSpawned = Instantiate(m_bulletPrefab);
        NetworkObject newGameObjectNetworkObject = objectSpawned.GetComponent<NetworkObject>();
        newGameObjectNetworkObject.transform.position = position;
        newGameObjectNetworkObject.SpawnWithOwnership(OwnerClientId);
        return objectSpawned;
        //return NetworkObjectSpawner.SpawnNewNetworkObject(
        //    m_bulletPrefab,
        //    position);
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

    private bool m_ShootBullet;
    private Unity.Netcode.Samples.ClientNetworkTransform m_ClientNetworkTransform;
    public override void OnNetworkSpawn()
    {
        m_ClientNetworkTransform = GetComponent<Unity.Netcode.Samples.ClientNetworkTransform>();
        m_PreviousTickPosition = m_cannonPosition.position;
        if (IsOwner)
        {
            m_ClientNetworkTransform.AuthoritativeStateCommitted += AuthoritativeStateCommitted;
        }

        if (IsServer && !IsOwner)
        {
            m_NonAuthTargetPosition = m_cannonPosition.position;
            m_ClientNetworkTransform.NonAuthoritativeTransformStasteUpdated += NonAuthoritativeTransformStateUpdated;
        }

        //if (IsServer)// && !IsOwner)
        //{
        //    NetworkManager.NetworkTickSystem.Tick += Server_Tick;
        //}

        base.OnNetworkSpawn();
    }

    public override void OnDestroy()
    {
        m_ClientNetworkTransform.AuthoritativeStateCommitted -= AuthoritativeStateCommitted;
        m_ClientNetworkTransform.NonAuthoritativeTransformStasteUpdated -= NonAuthoritativeTransformStateUpdated;
        base.OnDestroy();
    }



    private void FireBullet(Vector3 startPosition)
    {
        if (!m_ShootBullet)
        {
            return;
        }
        SpawnNewBulletVfx();

        GameObject newBullet = GetNewBullet(startPosition);

        PrepareNewlySpawnedBulltet(newBullet);

        PlayShootBulletSoundClientRpc();

        m_ShootBullet = false;
    }



    public Vector3 m_PlayerCurrentMotion = Vector3.zero;


    private Vector3 m_NonAuthTargetPosition;
    private Vector3 m_PreviousTickPosition;

    private void NonAuthoritativeTransformStateUpdated(bool positionUpdated, bool rotationUpdated, bool scaleUpdated)
    {
        if (positionUpdated)
        {
            m_NonAuthTargetPosition = m_cannonPosition.position;
        }
    }


    private void AuthoritativeStateCommitted(bool positionUpdated, bool rotationUpdated, bool scaleUpdated)
    {
        if (m_PlayerShoot)
        {
            if (IsServer)
            {
                var targetPosition = ((m_PreviousTickPosition * 2) + m_cannonPosition.position) * 0.33333f;

                //var targetPosition = (m_PreviousTickPosition + m_cannonPosition.position) * 0.5f;
                targetPosition -= m_PlayerCurrentMotion;
                m_ShootBullet = true;
                FireBullet(targetPosition + VisualOffset);
            }
            else
            {
                FireNewBulletServerRpc(m_cannonPosition.position + m_PlayerCurrentMotion);
            }
            m_PlayerShoot = false;
        }

        if (positionUpdated)
        {
            m_PreviousTickPosition = m_cannonPosition.position;
        }
    }
}
