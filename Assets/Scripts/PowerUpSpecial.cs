using Unity.Netcode;
using UnityEngine;

public class PowerUpSpecial : NetworkBehaviour
{
    [SerializeField]
    private float _speed;

    [SerializeField]
    private int _timeToLive;

    private void Update()
    {
        if (IsServer)
        {
            transform.Translate(Vector3.left * _speed * Time.deltaTime);
        }
    }

    private void AutoDestroy()
    {
        Despawn();
    }

    public void Despawn()
    {
        // TODO: added to a pooling
        CancelInvoke();

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
