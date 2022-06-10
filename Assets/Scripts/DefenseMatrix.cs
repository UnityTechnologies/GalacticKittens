using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class DefenseMatrix : NetworkBehaviour, IDamagable
{
    [SerializeField]
    private float m_rotationSpeed;

    private void Update()
    {
        if (IsServer)
        {
            transform.Rotate(Vector3.forward * m_rotationSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (IsServer)
        {
            if (collider.TryGetComponent(out IDamagable damagable))
            {
                damagable.Hit(1);
                TurnOffMatrixClientRpc();
            }
        }
    }

    public void Hit(int damage)
    {
        TurnOffMatrixClientRpc();
    }

    [ClientRpc]
    private void TurnOffMatrixClientRpc()
    {
        gameObject.SetActive(false);
    }

    IEnumerator IDamagable.HitEffect()
    {
        return null;
    }
}