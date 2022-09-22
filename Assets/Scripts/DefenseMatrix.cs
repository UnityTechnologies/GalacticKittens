using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class DefenseMatrix : NetworkBehaviour, IDamagable
{
    public bool isShieldActive { get; private set; } = false;

    private SpriteRenderer m_spriteRenderer;
    private CircleCollider2D m_circleCollider2D;

    private void Start()
    {
        m_spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        m_circleCollider2D = gameObject.GetComponent<CircleCollider2D>();
    }

    public void Hit(int damage)
    {
        TurnOffMatrixClientRpc();
    }

    public void TurnOnShield()
    {
        isShieldActive = true;

        m_spriteRenderer.enabled = true;
        m_circleCollider2D.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!IsServer)
            return;

        if (collider.TryGetComponent(out IDamagable damagable))
        {
            damagable.Hit(1);
            TurnOffMatrixClientRpc();
        }
    }

    [ClientRpc]
    private void TurnOffMatrixClientRpc()
    {
        isShieldActive = false;

        m_spriteRenderer.enabled = false;
        m_circleCollider2D.enabled = false;
    }

    IEnumerator IDamagable.HitEffect()
    {
        return null;
    }
}