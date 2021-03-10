using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForAudioEnd : MonoBehaviour
{

    public GameObject NextObject;

    private void Start()
    {
        StartCoroutine(waitForSound());
    }

    IEnumerator waitForSound()
    {
        while (GetComponent<AudioSource>().isPlaying)
        {
            Time.timeScale = 0;
            yield return null;
        }

        Time.timeScale = 1;
        NextObject.SetActive(true);
    }

}
