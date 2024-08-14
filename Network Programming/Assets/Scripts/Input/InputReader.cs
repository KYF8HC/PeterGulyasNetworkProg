using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input Reader")]
public class InputReader : ScriptableObject, GameInput.IGameplayActions
{
    private GameInput gameInput;
    public event UnityAction<Vector2> MoveEvent = delegate { };
    public event UnityAction ShootEvent = delegate { };
    public event UnityAction SendEvent = delegate { };
    public event UnityAction<bool> BreakEvent = delegate { };

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnSend(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SendEvent?.Invoke();
        }
    }

    public void OnBreak(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            BreakEvent?.Invoke(true);
        }

        if (context.canceled)
        {
            BreakEvent?.Invoke(false);
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ShootEvent?.Invoke();
        }
    }

    private void OnEnable()
    {
        if (gameInput == null)
        {
            gameInput = new GameInput();
            gameInput.Gameplay.SetCallbacks(this);
            gameInput.Gameplay.Enable();
        }
    }
}