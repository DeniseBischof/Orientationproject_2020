#if UNITY_EDITOR_WIN
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class SkipAudio : MonoBehaviour
{
    private Dictionary<string, Action> keyActs = new Dictionary<string, Action>();
    private KeywordRecognizer recognizer;

    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        keyActs.Add("skip", skipAudio);
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

    private void skipAudio()
    {
        recognizer.Stop();
        audioSource.Stop();
    }
}
#endif
