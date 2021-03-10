#if UNITY_EDITOR_WIN
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class GoToNextPart : MonoBehaviour
{
    // Voice command vars
    private Dictionary<string, Action> keyActs = new Dictionary<string, Action>();
    private KeywordRecognizer recognizer;

    public string Option;

    public AudioSource audioSource;

    public GameObject nextPart;
    public GameObject oldPart;

    // Start is called before the first frame update
    void Start()
    {
        keyActs.Add(Option, startNextPart);
        recognizer = new KeywordRecognizer(keyActs.Keys.ToArray());
        recognizer.OnPhraseRecognized += OnKeywordsRecognized;
        recognizer.Start();

    }

    void OnKeywordsRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log("Command: " + args.text);
        keyActs[args.text].Invoke();
    }

    public void startNextPart()
    {
        StartCoroutine(FadeOutAudio.StartFade(audioSource, 1f, 0f));
        nextPart.SetActive(true);
        oldPart.SetActive(false);
        recognizer.Stop();
    }
}
#endif
