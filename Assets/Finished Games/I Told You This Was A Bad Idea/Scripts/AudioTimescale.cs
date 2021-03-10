using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTimescale : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<AudioSource>().isPlaying)
        {
            while (GetComponent<AudioSource>().isPlaying)
            {
                Time.timeScale = 0;

            }

            Time.timeScale = 1;

        }
    }
}
