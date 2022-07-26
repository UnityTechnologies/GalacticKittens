using UnityEngine;
using Unity.Netcode;

public class RotateNetworkObjectOnZaxis : NetworkBehaviour
{
    public float m_rotationSpeed = 1f;

    private void Update()
    {
        if (!IsServer)
            return;
        
        transform.Rotate(m_rotationSpeed * Time.deltaTime * Vector3.forward);
    }
}
