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

    [SerializeField]
    private BulletOwner m_owner;

    [HideInInspector]
    public GameObject m_Owner { get; set; } = null;

    private void Start()
    {
        if (m_owner == BulletOwner.player && IsServer)
        {
            ChangeBulletColorClientRpc(Color.white);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!IsServer)
            return;

        if (collider.TryGetComponent(out IDamagable damagable))
        {
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
