using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class CustomPlayerInputManager : MonoBehaviour
{
    //Get a new variable to access the player inputs in Unity 6's new input system
    public static PlayerInput PlayerInput;

    //Action Checks
    public static Vector2 Movement;

    //Jump Checks
    //This input checks are for celeste type movement freedoms
    public static bool JumpWasPressed = false;
    public static bool JumpIsHeld = false;
    public static bool JumpWasReleased = false;

    //Run Checks
    public static bool RunIsHeld = false;

    //Action Bindings
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _runAction;

    //This calls when the script is loaded
    //We use this for things like finding gameobjects and setting them to variables
    //We also use this for things like making a singleton script
    private void Awake()
    {
        //Bind the PlayerInput component
        PlayerInput = GetComponent<PlayerInput>();

        //Bind the actions
        _moveAction = PlayerInput.actions["Move"];
        _jumpAction = PlayerInput.actions["Jump"];
        _runAction = PlayerInput.actions["Sprint"];

        //We can always bind more actions later for things like combat
    }

    // Update is called once per frame
    void Update()
    {
        //Reads the vector2 values from our player movement into this variable
        Movement = _moveAction.ReadValue<Vector2>();

        //Reads bool values from the player input component into these variables
        JumpWasPressed = _jumpAction.WasPressedThisFrame();
        JumpIsHeld = _jumpAction.IsPressed();
        JumpWasReleased = _jumpAction.WasReleasedThisFrame();

        //Reads a bool value from the player input component into this variable
        RunIsHeld = _runAction.IsPressed();
    }
}
