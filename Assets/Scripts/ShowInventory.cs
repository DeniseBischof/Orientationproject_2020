using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class ShowInventory : MonoBehaviour
{
    public GameObject[] ObjectsInInventory;

    public AudioClip empty;
    bool objectsAreActive;

    AudioSource audioSource;

    // Voice command vars
    private Dictionary<string, Action> keyActs = new Dictionary<string, Action>();
    private KeywordRecognizer recognizer;

    void Start()
    {
        keyActs.Add("look at inventory", showObjects);
        keyActs.Add("show inventory", showObjects);
        keyActs.Add("whats in the inventory", showObjects);
        keyActs.Add("inventory", showObjects);
        keyActs.Add("whats in my bag", showObjects);
        keyActs.Add("bag", showObjects);
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

        for (int i = 0; i < ObjectsInInventory.Length; i++)
        {
            if (ObjectsInInventory[i].activeInHierarchy)
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

        if (objectsAreActive) { 
            foreach (GameObject gameobject in ObjectsInInventory)
            {
                gameobject.GetComponent<AudioSource>().spatialBlend = 0;
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

}
