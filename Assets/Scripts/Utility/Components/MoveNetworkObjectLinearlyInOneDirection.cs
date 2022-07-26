using Unity.Netcode;
using UnityEngine;

public class MoveNetworkObjectLinearlyInOneDirection : NetworkBehaviour
{
    public Vector3 direction = Vector3.right;

    public float speed = 2f;

    private void Update()
    {
        if(!IsServer)
            return;

        transform.Translate(speed * Time.deltaTime * direction);
    }
}