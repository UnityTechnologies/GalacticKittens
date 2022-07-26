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

    private void Start()
    {
        if (m_owner == BulletOwner.player && IsServer)
        {
            ChangeBulletColorClientRpc(characterData.color);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!IsServer)
            return;

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
