using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class LookAround : MonoBehaviour
{

    public GameObject[] ObjectsAroundYou;

    // Voice command vars
    private Dictionary<string, Action> keyActs = new Dictionary<string, Action>();
    private KeywordRecognizer recognizer;

    bool objectsAreActive;
    public AudioClip empty;

    AudioSource audioSource;

    void Start()
    {
        keyActs.Add("look around", showObjects);
        recognizer = new KeywordRecognizer(keyActs.Keys.ToArray());
        recognizer.OnPhraseRecognized += OnKeywordsRecognized;
        recognizer.Start();

        audioSource = GetComponent<AudioSource>();
    }

    void OnKeywordsRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log("Command: " + args.text);
        keyActs[args.text].Invoke();
    }

    public void showObjects()
    {
        
        StartCoroutine(waitForSound());

    }


    void Update()
    {   // this was for testing
        //        if (Input.anyKeyDown) {
        //            StartCoroutine(waitForSound());
        //    }
    }

    IEnumerator waitForSound()
    {

        this.GetComponent<AudioSource>().Play(0);


        while (GetComponent<AudioSource>().isPlaying)
        {
            yield return null;
        }

        GetActiveObjects();

        if (objectsAreActive)
        {
            foreach (GameObject gameobject in ObjectsAroundYou)
            {
                gameobject.GetComponent<AudioSource>().Play(0);

                while (gameobject.GetComponent<AudioSource>().isPlaying)
                {
                    yield return null;
                }
            }
        }
        else
        {
            audioSource.PlayOneShot(empty);
            while (GetComponent<AudioSource>().isPlaying)
            {
                yield return null;
            }
        }
    }

    private void GetActiveObjects()
    {
        for (int i = 0; i < ObjectsAroundYou.Length; i++)
        {
            if (ObjectsAroundYou[i].activeInHierarchy)
            {
                Debug.Log(objectsAreActive);
                objectsAreActive = true;
                Debug.Log(objectsAreActive);
                break;
            }
            else
            {
                objectsAreActive = false;
                Debug.Log(objectsAreActive);
                break;
            }
        }
    }
}



