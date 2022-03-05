using Unity.Netcode;
using UnityEngine;

public class BulletController : NetworkBehaviour
{
    private enum BulletOwner
    {
        enemy,
        player
    };

    public int damage;

    [SerializeField]
    private float speed;

    [SerializeField]
    private int _timeToLive;

    [SerializeField]
    private BulletOwner _owner;

    [Header("Set in runtime")]
    public CharacterDataSO characterData;

    public Vector3 direction { get; set; } = Vector3.right;

    public GameObject m_Owner { get; set; } = null;

    private void Start()
    {
        if (_owner == BulletOwner.player && IsServer)
        {
            ChangeBulletColorClientRpc(characterData.color);
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            transform.Translate(direction * speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!IsServer)
            return;

        if (_owner == BulletOwner.player)
        {
            if (collider.TryGetComponent(out IDamagable damagable))
            {
                damagable.Hit(damage);

                // For the final score
                characterData.enemiesDestroyed++;
                Despawn();
            }
        }
        else
        {
            if (collider.TryGetComponent(out IDamagable damagable))
            {
                damagable.Hit(damage);
                Despawn();
            }
        }
    }

    private void AutoDestroy()
    {
        Despawn();
    }

    [ClientRpc]
    private void ChangeBulletColorClientRpc(Color newColor)
    {
        GetComponent<SpriteRenderer>().color = newColor;
    }

    public void Despawn()
    {
        if(NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            Invoke(nameof(AutoDestroy), _timeToLive);

        base.OnNetworkSpawn();
    }
}
