using UnityEngine;
using UnityEngine.InputSystem;

public class TouchManager : MonoBehaviour
{
    private PlayerInput playerInput;

    private InputAction touchTapAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        touchTapAction = playerInput.actions.FindAction("Tap");
    }

    private void OnEnable()
    {
        touchTapAction.performed += TouchPressed;
    }

    private void OnDisable()
    {
        touchTapAction.performed -= TouchPressed;
    }
    

    private void TouchPressed(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        Debug.Log(value + " tap detectado");

        //context.ReadValueAsButton();
    }
}
