using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class pemancarEvants : MonoBehaviour
{
    public  static event Action saattombolditekan;
    
    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
           Debug.Log("Tombol ditekan");
           saattombolditekan?.Invoke();
        }
    }
}
