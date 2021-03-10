#if UNITY_EDITOR_WIN
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class DestroyThisObject : MonoBehaviour
{
    // Voice command vars
    private Dictionary<string, Action> keyActs = new Dictionary<string, Action>();
    private KeywordRecognizer recognizer;

    public string objectName;

    AudioSource audioSource;
    public AudioClip itemDestroyed;


    // Start is called before the first frame update
    void Start()
    {
        keyActs.Add("destroy " + objectName, destroyItem);
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

    public void destroyItem()
    {
        Debug.Log("You destroyed " + objectName);
        StartCoroutine(waitForSound());

    }

    IEnumerator waitForSound()
    {
        audioSource.spatialBlend = 0;
        audioSource.PlayOneShot(itemDestroyed);

        while (GetComponent<AudioSource>().isPlaying)
        {
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
#endif
