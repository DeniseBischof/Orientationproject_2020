#if UNITY_EDITOR_WIN
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class TakeObject : MonoBehaviour
{
    public string objectName;

    public GameObject turnOff;
    public GameObject turnOn;

    AudioSource audioSource;
    public AudioClip itemTaken;

    // Voice command vars
    private Dictionary<string, Action> keyActs = new Dictionary<string, Action>();
    private KeywordRecognizer recognizer;

    void Start()
    {
        keyActs.Add("take " + objectName, takeItem);
        keyActs.Add("look at " + objectName, lookAt);
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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void takeItem()
    {
        StartCoroutine(waitForSound());

        Debug.Log("You took " + objectName);
    }

    public void lookAt()
    {
        Debug.Log("You looked at " + objectName);
    }

    IEnumerator waitForSound()
    {
        audioSource.spatialBlend = 0;
        audioSource.PlayOneShot(itemTaken);

        while (GetComponent<AudioSource>().isPlaying)
        {
            yield return null;
        }

        turnOn.SetActive(true);
        turnOff.SetActive(false);
    }
}
#endif
