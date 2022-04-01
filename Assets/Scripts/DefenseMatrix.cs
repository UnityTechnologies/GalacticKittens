using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class DefenseMatrix : NetworkBehaviour, IDamagable
{
    [SerializeField]
    private float m_rotationSpeed;

    void Update()
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
            // print($"DefenseMatrix collide with:: {collider.tag}");
            if (collider.TryGetComponent(out IDamagable damagable))
            {
                damagable.Hit(1);
                TurnOffMatrixClientRpc();
            }
        }
    }

    [ClientRpc]
    private void TurnOffMatrixClientRpc()
    {
        gameObject.SetActive(false);
    }

    public void Hit(int damage)
    {
        TurnOffMatrixClientRpc();
    }

    public IEnumerator HitEffect()
    {
        throw new System.NotImplementedException();
    }
}