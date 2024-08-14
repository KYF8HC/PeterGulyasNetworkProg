using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : NetworkBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private Sprite bulletSprite;
    [SerializeField] private Sprite explosionSprite;
    [SerializeField] private CapsuleCollider2D capsuleCollider2D;
    
    public Rigidbody2D BulletRigidbody2D;
    private GameObject _instigator;
    private SpriteRenderer _spriteRenderer;
    private bool _bShouldExplode;

    private void Awake()
    {
        BulletRigidbody2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        BulletRigidbody2D.gravityScale = 0;
    }

    private void Update()
    {
        if (!_bShouldExplode) return;
        if (transform.localScale.x >= 1) return;
        transform.localScale += new Vector3(Time.deltaTime, Time.deltaTime, Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == _instigator)
            return;

        if (!NetworkManager.Singleton.IsServer || !NetworkObject.IsSpawned)
            return;

        if (other.gameObject.CompareTag("Obstacle"))
        {
            ExplodeRpc();
            TimerManager.Instance.SetTimer(1f, () => DestroyBullet());
            return;
        }

        if (!other.gameObject.TryGetComponent(out AttributeComponent attributeComponent))
            return;
        
        attributeComponent.ApplyHealthChange(-damage);
        TimerManager.Instance.SetTimer(1f, () => DestroyBullet());
        ExplodeRpc();
    }
    
    public void StartLifetimeTimer()
    {
        TimerManager.Instance.SetTimer(5f, () => DestroyBullet());
    }

    public void SetInstigator(GameObject instigator)
    {
        _instigator = instigator;
    }

    private void DestroyBullet()
    {
        if (!NetworkObject.IsSpawned)
            return;
        
        ResetBulletRpc();
        NetworkObject.Despawn();
    }

    public void SetVelocity(Vector2 velocity)
    {
        if (IsServer)
        {
            BulletRigidbody2D.velocity = velocity;
            SetVelocityRPC(velocity);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetVelocityRPC(Vector2 velocity)
    {
        //No need to set velocity for host since we call this from the server and the host is both server and client
        if(!IsHost)
            BulletRigidbody2D.velocity = velocity;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ResetBulletRpc()
    {
        capsuleCollider2D.enabled = true;
        _bShouldExplode = false;
        _spriteRenderer.sprite = bulletSprite;
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        BulletRigidbody2D.bodyType = RigidbodyType2D.Dynamic;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ExplodeRpc()
    {
        capsuleCollider2D.enabled = false;
        _bShouldExplode = true;
        _spriteRenderer.sprite = explosionSprite;
        transform.localScale = Vector3.zero;
        BulletRigidbody2D.bodyType = RigidbodyType2D.Static;
    }
}