using System;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : NetworkBehaviour
{
    private const float Tolerance = 0.001f;

    [SerializeField] private InputReader inputReader;
    [SerializeField] private GameObject objectToSpawn;
    [SerializeField] private float rotateSpeed = 200.0f;
    [SerializeField] private float topSpeed = 50.0f;
    [SerializeField] private float acceleration = 1.0f;
    [SerializeField] private float breakForce = 2.0f;
    [SerializeField] private float magnitudeTolerance = 1.0f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject healthBarPrefab;

    private NetworkObjectPool _objectPool;

    private Vector2 _moveInput;
    private Rigidbody2D _rigidbody2D;
    private Transform _mainCamera;
    private HealthBar _healthBarInstance;
    private bool _bIsBreaking = false;

    //Client
    private float _oldMoveForce;
    private float _oldSpin;

    //Server 
    private float _thrust;
    private float _spin;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _mainCamera = GameObject.Find("Main Camera").transform;
        _objectPool = GameObject.Find("NetworkObjectPool").GetComponent<NetworkObjectPool>();

        _rigidbody2D.gravityScale = 0;
        _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;

        if (inputReader != null && IsLocalPlayer)
        {
            inputReader.MoveEvent += OnMove;
            inputReader.ShootEvent += ServerFireRpc;
            inputReader.BreakEvent += OnBreak;
        }

        _healthBarInstance = Instantiate(healthBarPrefab, Vector3.zero, Quaternion.identity).GetComponent<HealthBar>();
        _healthBarInstance.SetOwner(transform);

        var attributeComponent = GetComponent<AttributeComponent>();
        attributeComponent.SetHealthBar(_healthBarInstance);
        attributeComponent.Initialize();
        attributeComponent.OnDeath += Respawn;
    }

    private void Update()
    {
        if (IsClient)
        {
            UpdateClient();
        }

        if (IsServer)
        {
            UpdateServer();
        }

        if (_bIsBreaking)
        {
            ServerBreak();
        }
    }

    private void LateUpdate()
    {
        if (IsLocalPlayer)
        {
            var pos = transform.position;
            pos.z = -50;
            _mainCamera.transform.position = pos;
        }
    }

    private void UpdateClient()
    {
        if (!IsLocalPlayer)
        {
            return;
        }

        var lSpin = -_moveInput.x;
        var moveForce = _moveInput.y;

        if (Math.Abs(_oldMoveForce - moveForce) > Tolerance || Math.Abs(_oldSpin - lSpin) > Tolerance)
        {
            ServerThrustRpc(moveForce, lSpin);
            _oldMoveForce = moveForce;
            _oldSpin = lSpin;
        }
    }

    private void UpdateServer()
    {
        if (_bIsBreaking)
            return;

        var rotate = _spin * rotateSpeed;

        _rigidbody2D.angularVelocity = rotate;

        if (_thrust == 0) return;
        var accel = acceleration;

        var thrustVec = transform.right * (_thrust * accel);
        _rigidbody2D.AddForce(thrustVec);

        var top = topSpeed;

        if (_rigidbody2D.velocity.magnitude > top)
        {
            _rigidbody2D.velocity = _rigidbody2D.velocity.normalized * top;
        }
    }
    
    private void ServerBreak()
    {
        _rigidbody2D.velocity *= (1 - breakForce * Time.deltaTime);

        if (_rigidbody2D.velocity.magnitude < magnitudeTolerance)
        {
            _rigidbody2D.velocity = Vector2.zero;
        }
    }
    
    private void Fire(Vector3 direction)
    {
        var rotation = transform.rotation * Quaternion.Euler(0, 0, -90);
        var bulletGo = _objectPool.GetNetworkObject(bulletPrefab, Vector3.zero, rotation).gameObject;
        bulletGo.transform.position = transform.position + direction;

        var velocity = _rigidbody2D.velocity;
        velocity += (Vector2)(direction) * 10;
        
        bulletGo.GetComponent<NetworkObject>().Spawn(true);
        
        var bullet = bulletGo.GetComponent<Bullet>();
        bullet.BulletRigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        bullet.SetInstigator(gameObject);
        bullet.SetVelocity(velocity);
        bullet.StartLifetimeTimer();
    }
    
    private void Respawn()
    {
        transform.position = Vector3.zero;
    }
    
    private void OnMove(Vector2 input)
    {
        _moveInput = input;
    }

    private void OnBreak(bool lIsBreaking)
    {
        OnBreakRPC(lIsBreaking);
    }

    public override void OnNetworkDespawn()
    {
        _healthBarInstance.Despawn();
        base.OnNetworkDespawn();
    }
        
    [Rpc(SendTo.Server)]
    private void OnBreakRPC(bool lIsBreaking)
    {
        _bIsBreaking = lIsBreaking;
    }

    [Rpc(SendTo.Server)]
    public void ServerThrustRpc(float thrusting, float lSpin)
    {
        _thrust = thrusting;
        _spin = lSpin;
    }

    [Rpc(SendTo.Server)]
    public void ServerFireRpc()
    {
        var right = transform.right;
        Fire(right);
    }
    
}