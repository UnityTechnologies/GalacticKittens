using UnityEngine;
using Unity.Netcode;

public class SpaceGhostEnemyBehavior : BaseEnemyBehavior
{
    [SerializeField]
    private AudioClip m_damageClip;

    private bool m_IsFlashingFromHit = false;
    private float m_FlashFromHitTime = 0.7f;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            m_EnemyHealthPoints.OnValueChanged += OnEnemyHealthPointsChange;

        base.OnNetworkSpawn();
    }

    private void OnDisable()
    {
        if (IsServer)
            m_EnemyHealthPoints.OnValueChanged -= OnEnemyHealthPointsChange;
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (IsServer)
        {
            base.Update();
        }
    }

    protected override void UpdateActive()
    {
        if (m_IsFlashingFromHit)
        {
            m_FlashFromHitTime -= Time.deltaTime;
            if (m_FlashFromHitTime <= 0f)
            {
                ToggleEnemyHitStatusClientRpc();
            }
        }

        MoveEnemy();
    }

    protected override void UpdateDefeatedAnimation()
    {
        PowerUpSpawnController.instance.OnPowerUpSpawn(transform.position);
        NetworkObjectSpawner.SpawnNewNetworkObject(m_VfxExplosion, transform.position);

        m_EnemyState.Value = EnemyState.defeated;
    }

    private void OnTriggerEnter2D(Collider2D otherObject)
    {
        // Only react to trigger on the server
        if (!IsServer)
            return;

        // check if it's collided with a player spaceship
        var spacheshipController = otherObject.gameObject.GetComponent<PlayerShipController>();
        if (spacheshipController != null)
        {
            // tell the spaceship that it's taken damage
            spacheshipController.Hit(1);

            // enemy explodes when it collides with the a player's ship
            m_EnemyState.Value = EnemyState.defeatAnimation;
        }
    }

    [ClientRpc]
    private void PlayEnemyDamageSoundClientRpc()
    {
        AudioManager.Instance.PlaySoundEffect(m_damageClip);
    }

    [ClientRpc]
    private void ToggleEnemyHitStatusClientRpc()
    {
        m_IsFlashingFromHit = !m_IsFlashingFromHit;
        GetComponent<SpriteRenderer>().color = m_IsFlashingFromHit ? Color.red : Color.white;
    }

    private void OnEnemyHealthPointsChange(int oldHP, int newHP)
    {
        // if enemy's health is 0, then time to start enemy dead animation
        if (newHP <= 0)
        {
            m_EnemyState.Value = EnemyState.defeatAnimation;
        }
    }

    public override void Hit(int damage)
    {
        base.Hit(damage);

        PlayEnemyDamageSoundClientRpc();
        ToggleEnemyHitStatusClientRpc();
    }
}
