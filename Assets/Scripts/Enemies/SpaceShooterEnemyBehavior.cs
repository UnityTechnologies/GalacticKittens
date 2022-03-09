using Unity.Netcode;
using UnityEngine;

public class SpaceShooterEnemyBehavior : BaseEnemyBehavior
{
    [SerializeField]
    public GameObject m_EnemyBulletPrefab;

    [SerializeField]
    private NetworkVariable<float> m_ShootingCooldown =
        new NetworkVariable<float>(NetworkVariableReadPermission.Everyone, 3.5f);
        
    [SerializeField]
    private AudioClip _shootClip;

    private float m_CurrentCooldownTime = 0f;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            m_EnemyHealthPoints.OnValueChanged += OnEnemyHealthPointsChange;

        m_EnemyMovementType = GetRandomEnemyMovementType();

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
        m_CurrentCooldownTime += Time.deltaTime;
        if (m_CurrentCooldownTime >= m_EnemyLifetime.Value)
        {
            DespawnEnemy();
        }
        else
        {
            MoveEnemy();
        }

        m_ShootingCooldown.Value -= Time.deltaTime;
        if (m_ShootingCooldown.Value <= 0f)
        {
            m_ShootingCooldown.Value = m_CurrentCooldownTime;
            ShootLaserServerRpc();
        }
    }

    protected override void UpdateDefeatedAnimation()
    {
        PowerUpSpawnController.instance.OnPowerUpSpawn(transform.position);
        NetworkSpawnController.SpawnHelper(m_VfxExplosion, this.transform.position);

        m_EnemyState.Value = EnemyState.defeated;
    }

    [ServerRpc]
    private void ShootLaserServerRpc()
    {
        // print($"Fire:: {NetworkObject.OwnerClientId}");
        var newEnemyLaser = NetworkSpawnController.SpawnHelper(m_EnemyBulletPrefab);
        PlayShootAudioClientRpc();

        var bulletController = newEnemyLaser.GetComponent<BulletController>();
        if (bulletController != null)
        {
            bulletController.m_Owner = gameObject;
            bulletController.direction = Vector2.left;
        }

        newEnemyLaser.transform.position = this.gameObject.transform.position;
        
    }

    [ClientRpc]
    private void PlayShootAudioClientRpc()
    {
        AudioManager.Instance.PlaySound(_shootClip);
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

        // check if it's collided with a player's bullet
        var shipBulletBehavior = otherObject.gameObject.GetComponent<BulletController>();
        if (shipBulletBehavior != null && shipBulletBehavior.m_Owner != this.gameObject)
        {
            // if so, take one health point away from enemy
            m_EnemyHealthPoints.Value -= 1;
        }
    }

    private void OnEnemyHealthPointsChange(int oldHP, int newHP)
    {
        // if enemy's health is 0, then time to start enemy dead animation
        if (newHP <= 0)
        {
            m_EnemyState.Value = EnemyState.defeatAnimation;
        }
    }
}
