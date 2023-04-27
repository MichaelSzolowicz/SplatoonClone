
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputsTest : MonoBehaviour
{
    /*
    private Rigidbody rigidbody;
    private PlayerInputActions playerInputActions;

    private void FixedUpdate()
    {
        Vector2 moveInput = playerInputActions.Player.Move.ReadValue<Vector2>();
        rigidbody.AddForce(new Vector3(moveInput.x, 0.0f, moveInput.y) * 5.0f, ForceMode.Force);
    }

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            Debug.Log("Player jumped: " + context.phase);
            rigidbody.AddForce(Vector3.up * 5.0f, ForceMode.Impulse);
        }
        
    }

    /*
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();
        rigidbody.AddForce(new Vector3(moveInput.x, 0.0f, moveInput.y) * 5.0f, ForceMode.Force);
    }
    */
}

