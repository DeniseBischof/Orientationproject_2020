#if UNITY_EDITOR_WIN
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetMicrophone : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        foreach (var device in Microphone.devices)
        {
            Debug.Log("Name: " + device);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
#endif