using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NewInputTest : MonoBehaviour
{
    DefaultActions actions;
    Vector2 input;

    private void Awake()
    {
        actions = new DefaultActions();
        actions.Enable();

        actions.Player.ButtonPress.performed += OnButtonPress;
    }

    private void Update()
    {
        input = actions.Player.Movement.ReadValue<Vector2>();

        if(input.x > 0) { print("Player pressed d: "+input.x);  }
        if (input.x < 0) { print("Player pressed a: " + input.x); }
        if (input.y > 0) { print("Player pressed w: " + input.y); }
        if (input.y < 0) { print("Player pressed s: " + input.y); }

        /*
        if(actions.Player.ButtonPress.WasPressedThisFrame())
        {
            print("Player pressed space button");
        }*/
    }

    private void OnButtonPress(InputAction.CallbackContext context)
    {
        print("Player pressed space button");
    }
}
