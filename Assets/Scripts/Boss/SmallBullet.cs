using Unity.Netcode;
using UnityEngine;

public class SmallBullet : NetworkBehaviour
{
    [SerializeField]
    private float m_speed = 8f;
    private int m_damage = 1;

    private void Update()
    {
        if (!IsServer)
            return;

        transform.Translate(Vector3.up * m_speed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (IsServer && collider.TryGetComponent(out IDamagable damagable))
        {
            damagable.Hit(m_damage);

            NetworkObjectDespawner.DespawnNetworkObject(NetworkObject);
        }
    }
}
