using Unity.Netcode;
using UnityEngine;

public class BulletController : NetworkBehaviour
{
    private enum BulletOwner
    {
        enemy,
        player
    };

    public int damage = 1;

    [HideInInspector]
    public CharacterDataSO characterData;

    [SerializeField]
    private BulletOwner m_owner;

    [HideInInspector]
    public GameObject m_Owner { get; set; } = null;

    public GameObject ServerCollider;

    private NetworkObject m_ServerColliderInstance;


    private void Start()
    {
        if (m_owner == BulletOwner.player && IsServer)
        {
            ChangeBulletColorClientRpc(characterData.color);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer && ServerCollider != null)
        {
            var objectInstance = Instantiate(ServerCollider);
            m_ServerColliderInstance = objectInstance.GetComponent<NetworkObject>();
            m_ServerColliderInstance.Spawn();
            m_ServerColliderInstance.TrySetParent(NetworkObject, false);
        }
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && m_ServerColliderInstance != null && m_ServerColliderInstance.IsSpawned)
        {
            m_ServerColliderInstance.Despawn();
        }

        base.OnNetworkDespawn();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!IsServer)
            return;
    }

    public void OnCollide(Collider2D collider)
    {
        if (!IsServer || !IsSpawned)
        {
            return;
        }

        if (collider.TryGetComponent(out IDamagable damagable))
        {
            if (m_owner == BulletOwner.player)
            {
                // For the final score
                characterData.enemiesDestroyed++;
            }

            damagable.Hit(damage);

            NetworkObjectDespawner.DespawnNetworkObject(NetworkObject);
        }

    }

    [ClientRpc]
    private void ChangeBulletColorClientRpc(Color newColor)
    {
        GetComponent<SpriteRenderer>().color = newColor;
    }
}
