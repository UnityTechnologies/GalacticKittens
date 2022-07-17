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
    private float m_speed = 12f;

    [SerializeField]
    private BulletOwner m_owner;

    [HideInInspector]
    public Vector3 direction { get; set; } = Vector3.right;

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

            Despawn();
        }
    }

    [ClientRpc]
    private void ChangeBulletColorClientRpc(Color newColor)
    {
        GetComponent<SpriteRenderer>().color = newColor;
    }

    private void Despawn()
    {
        if(NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }
}
