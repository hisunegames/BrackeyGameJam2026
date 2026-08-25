using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhoneController : MonoBehaviour
{
    [SerializeField] GameObject map;
    [SerializeField] GameObject notepad;
    [SerializeField] GameObject contentText;
    
    private InputAction mapAction;
    private InputAction notepadAction;
    private List<string> keywords;
    
    void Start()
    {
        mapAction = InputSystem.actions.FindAction("Map");
        notepadAction = InputSystem.actions.FindAction("Notepad");
        keywords = new List<string>();
        // Test text
        keywords.Add("Dark eyes");
        keywords.Add("A man with a strange hat");
        keywords.Add("Chicken");

        foreach (var keyword in keywords)
        {
            TMP_Text tmp = contentText.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.text = tmp.text + keyword + "\n";
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (mapAction.triggered)
        {
            Debug.unityLogger.Log("Map");
            map.SetActive(true);
            notepad.SetActive(false);
        } else if (notepadAction.triggered)
        {
            Debug.unityLogger.Log("Notepad");
            map.SetActive(false);
            notepad.SetActive(true);
        }
    }
}
