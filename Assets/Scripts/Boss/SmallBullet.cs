using Unity.Netcode;
using UnityEngine;

public class SmallBullet : NetworkBehaviour
{
    [SerializeField]
    private float _speed;
    private int _damage = 1;

    private void Update()
    {
        if (IsServer)
        {
            transform.Translate(Vector3.up * _speed * Time.deltaTime, Space.Self);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (IsServer)
        {            
            if (collider.TryGetComponent(out IDamagable damagable))
            {
                damagable.Hit(_damage);
                Despawn();
            }   
        }
    }

    private void Despawn()
    {
        NetworkObject.Despawn();
    }

}
