// using Unity.VisualScripting;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
public class Z_VRInput : MonoBehaviour
{
    public static Z_VRInput Instance;

    private void Awake()
    {
        Instance = this;
    }
    InputAction buttonA;
    InputAction buttonB;

    public bool isAPressing = false;
    public bool isBPressing = false;

    public UnityEvent onPrimaryButtonPressed;
    public UnityEvent onSecondaryButtonPressed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ///==========> onPrimaryButtonPressed A <========== 
        buttonA = new InputAction(type: InputActionType.Button, binding: "<XRController>{RightHand}/primaryButton");
        buttonB = new InputAction(type: InputActionType.Button, binding: "<XRController>{RightHand}/secondaryButton");

        buttonA.started += v =>
        {
           onPrimaryButtonPressed?.Invoke();
           isAPressing = true;
        

        }; 
        buttonA.canceled += v =>
        {
            isAPressing= false;
        };
        buttonA.Enable();

        buttonB.started += v =>
        {
            onSecondaryButtonPressed?.Invoke();
            isBPressing = true;
        };
        buttonB.canceled += v =>
        {
            isBPressing = false;
        };
        buttonB.Enable();
    }
}