using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootStepsSound : MonoBehaviour
{

    AudioSource audioSource;
    public AudioClip[] FootSteps;


    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void playFootstepSound()
    {
        audioSource.clip = FootSteps[Random.Range(0, FootSteps.Length)];
        audioSource.Play();
    }
}
