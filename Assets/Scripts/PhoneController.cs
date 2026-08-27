using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhoneController : MonoBehaviour
{
    [SerializeField] GameObject map;
    [SerializeField] GameObject notepad;
    
    private InputAction mapAction;
    private InputAction notepadAction;
    
    void Start()
    {
        mapAction = InputSystem.actions.FindAction("Map");
        notepadAction = InputSystem.actions.FindAction("Notepad");
    }

    // Update is called once per frame
    void Update()
    {
        if (mapAction.triggered)
        {
            Debug.unityLogger.Log("Map");
        } else if (notepadAction.triggered)
        {
            Debug.unityLogger.Log("Notepad");
        }
    }
}
