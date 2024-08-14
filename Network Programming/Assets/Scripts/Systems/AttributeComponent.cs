using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class AttributeComponent : NetworkBehaviour
{
    public event UnityAction OnDeath = delegate { };
    [SerializeField] private float maxHealth;
    public NetworkVariable<float> health = new();

    private HealthBar _healthBar;

    public void Initialize()
    {
        if (!IsServer)
            return;
        health.Value = maxHealth;
        health.OnValueChanged += OnHealthChanged;
        UpdateHealthBarRpc(maxHealth);
    }

    private void OnHealthChanged(float previousvalue, float newvalue)
    {
        UpdateHealthBarRpc(newvalue);
    }

    public void SetHealthBar(HealthBar healthBar)
    {
        _healthBar = healthBar;
    }

    public void ApplyHealthChange(float amount)
    {
        health.Value += amount;
        health.Value = Mathf.Clamp(health.Value, 0, maxHealth);
        if (health.Value <= 0)
        {
            OnDeath?.Invoke();
            health.Value = maxHealth;
        }
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateHealthBarRpc(float newHealth)
    {
        if (_healthBar)
            _healthBar.SetHealth(newHealth, maxHealth);
    }
}