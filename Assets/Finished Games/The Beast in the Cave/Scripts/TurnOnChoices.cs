using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOnChoices : MonoBehaviour
{
    [SerializeField]
    private GameObject[] Choices;
    public float SecondsToWait = 0;

    private void Start()
    {
        StartCoroutine(waitForSound());
        Debug.Log("waiting for sound from: " + this.name.ToString());
    }

    IEnumerator waitForSound()
    {
        while (GetComponent<AudioSource>().isPlaying)
        {
            yield return null;
        }

        yield return new WaitForSeconds(SecondsToWait);

        for (int i = 0; i < Choices.Length; i++)
        {
            Debug.Log("turning on: " + Choices[i].ToString());
            Choices[i].SetActive(true);
        }

    }

}
