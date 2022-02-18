using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class DefenseMatrix : NetworkBehaviour, IDamagable
{
    [SerializeField]
    private float _rotationSpeed;

    void Update()
    {
        if (IsServer)
        {
            transform.Rotate(Vector3.forward * _rotationSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // print($"DefenseMatrix collide with:: {collider.tag}");
        if (collider.TryGetComponent(out IDamagable damagable))
        {
            damagable.Hit(1);
            TurnOffMatrixClientRpc();
        }
    }

    [ClientRpc]
    private void TurnOffMatrixClientRpc()
    {
        this.gameObject.SetActive(false);
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
