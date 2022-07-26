using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class SuperLaser : NetworkBehaviour
{
    [SerializeField]
    private int _damage;

    [SerializeField]
    private float _readyTime;

    [SerializeField]
    private float _stillTime;

    private Animator _animatorController;

    private IEnumerator LaserEffect()
    {
        yield return new WaitForSeconds(_readyTime);

        _animatorController.SetBool("Fire", true);

        yield return new WaitForSeconds(_stillTime);

        _animatorController.SetBool("Fire", false);

        yield return new WaitForSeconds(_animatorController.GetCurrentAnimatorStateInfo(0).length);

        NetworkObjectDespawner.DespawnNetworkObject(NetworkObject);

    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (IsServer)
        {
            if (collider.GetComponent<IDamagable>() != null)
            {
                collider.GetComponent<IDamagable>().Hit(_damage);
            }

            // TODO: Remove
            if (collider.CompareTag("Player"))
            {
                collider.GetComponent<PlayerShipController>().Hit(_damage);
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(LaserEffect());
            _animatorController = GetComponent<Animator>();
        }
        base.OnNetworkSpawn();
    }
}
