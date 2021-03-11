using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class turnOffAfterPlay : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(waitForSound());
    }

    IEnumerator waitForSound()
    {
        while (GetComponent<AudioSource>().isPlaying)
        {
            yield return null;
        }


            gameObject.SetActive(false);
        

    }
}
