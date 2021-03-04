using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOnChoices : MonoBehaviour
{

    public GameObject Choice;

    private void Start()
    {
        StartCoroutine(waitForSound());
    }

    IEnumerator waitForSound()
    {
        while (GetComponent<AudioSource>().isPlaying)
        {
            yield return null;
        }

        Choice.SetActive(true);
    }

}
