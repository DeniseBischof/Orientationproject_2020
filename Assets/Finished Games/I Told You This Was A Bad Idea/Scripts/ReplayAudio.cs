#if UNITY_EDITOR_WIN
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class ReplayAudio : MonoBehaviour
{
    public GameObject[] ObjectsAroundYou;

    // Voice command vars
    private Dictionary<string, Action> keyActs = new Dictionary<string, Action>();
    private KeywordRecognizer recognizer;

    public AudioSource audioSource;

    void Start()
    {
        keyActs.Add("again", replayAudio);
        keyActs.Add("can you repeat that", replayAudio);
        keyActs.Add("repeat that", replayAudio);
        keyActs.Add("repeat", replayAudio);
        keyActs.Add("repeat that please", replayAudio);
        recognizer = new KeywordRecognizer(keyActs.Keys.ToArray());
        recognizer.OnPhraseRecognized += OnKeywordsRecognized;
        recognizer.Start();

        audioSource = this.GetComponent<AudioSource>();
    }

    void OnKeywordsRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log("Command: " + args.text);
        keyActs[args.text].Invoke();
    }

    public void replayAudio()
    {
        audioSource.Play();
    }
}
#endif
