using Unity.Netcode;
using UnityEngine;

public class BulletColliderController : NetworkBehaviour
{
    private BulletController m_BulletController;


    public override void OnNetworkObjectParentChanged(NetworkObject parentNetworkObject)
    {
        if (parentNetworkObject != null)
        {
            m_BulletController = parentNetworkObject.GetComponent<BulletController>();
        }
        base.OnNetworkObjectParentChanged(parentNetworkObject);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!IsServer || m_BulletController == null)
            return;

        m_BulletController.OnCollide(collider);
    }
}
