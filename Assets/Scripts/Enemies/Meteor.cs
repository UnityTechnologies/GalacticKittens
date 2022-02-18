using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Meteor : NetworkBehaviour, IDamagable
{
    [SerializeField]
    private int _damage;

    [SerializeField]
    private float _speed;

    [SerializeField]
    private float _rotationSpeed;

    [SerializeField]
    private int _health;

    [SerializeField]
    private float _timeToLive;

    [SerializeField]
    private GameObject _vfxExplosion;

    [SerializeField]
    private Sprite[] _meteors;

    [SerializeField]
    private GameObject _meteorSprite;

    [SerializeField]
    SpriteRenderer m_sprite;

    [SerializeField]
    float m_hitEffectDuration;

    [Header("Range for random scale")]
    [SerializeField]
    private float _scaleMin;

    [SerializeField]
    private float _scaleMax;

    private void Start()
    {
        // Randomly select the sprite to use 
        GetComponentInChildren<SpriteRenderer>().sprite = _meteors[Random.Range(0, _meteors.Length)];

        // Randomly scale the meteor
        float randomScale = Random.Range(_scaleMin, _scaleMax);
        transform.localScale = new Vector3(randomScale, randomScale, 1f);
    }

    private void Update()
    {
        if (IsServer)
        {
            _meteorSprite.transform.Rotate(Vector3.forward * _rotationSpeed * Time.deltaTime);
            transform.Translate(Vector3.left * _speed * Time.deltaTime, Space.Self);
        }
    }

    private void AutoDestroy()
    {
        Despawn();
    }

    private void Despawn()
    {
        CancelInvoke();

        if (NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.TryGetComponent(out IDamagable damagable))
        {
            // Hit the object that collide with me
            damagable.Hit(_damage);

            // Hit me too!
            Hit(_damage);
        }
    }

    public IEnumerator HitEffect()
    {
        bool active = false;
        float timer = 0;
        while (timer < m_hitEffectDuration)
        {
            active = !active;
            m_sprite.material.SetInt("_Hit", active ? 1 : 0);
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }

        m_sprite.material.SetInt("_Hit", 0);
    }

    public void Hit(int damage)
    {
        _health -= damage;
        if (_health <= 0)
        {
            PowerUpSpawnController.instance.OnPowerUpSpawn(transform.position);
            NetworkSpawnController.SpawnHelper(_vfxExplosion, this.transform.position);
            Despawn();
        }

        StopCoroutine(HitEffect());
        StartCoroutine(HitEffect());
    }

    public override void OnNetworkSpawn()
    {
        // We use the invoke for destroying this GameObject in case it doesn't hit anything
        if (IsServer)
            Invoke(nameof(AutoDestroy), _timeToLive);

        base.OnNetworkSpawn();
    }
}
