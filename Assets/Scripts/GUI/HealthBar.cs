using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : NetworkBehaviour
{
    [SerializeField] private Slider healthBar;
    private Transform _owner;

    public void SetHealth(float currentHealth, float maxHealth)
    {
        healthBar.value = currentHealth / maxHealth;
    }

    public void SetOwner(Transform owner)
    {
        _owner = owner;
    }

    public void Despawn()
    {
        if (!NetworkObject.IsSpawned)
        {
            return;
        }
        NetworkObject.Despawn(true);
    }

    private void Update()
    {
        if (_owner)
        {
            //Could be done in Player script on late update with camera
            transform.position = _owner.position;
        }
    }
}