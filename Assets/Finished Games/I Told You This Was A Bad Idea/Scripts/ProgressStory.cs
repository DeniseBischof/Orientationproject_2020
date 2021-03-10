#if UNITY_EDITOR_WIN
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class ProgressStory : MonoBehaviour
{
    private Dictionary<string, Action> keyActs = new Dictionary<string, Action>();
    private KeywordRecognizer recognizer;

    private AudioSource audioSource;

    public bool TurnOnNextLevel;

    [SerializeField]
    private string[] keyWords;
    [SerializeField]
    private AudioClip[] audioAnswers;

    // Start is called before the first frame update
    void Start()
    {
/*        foreach (string keyword in keyWords)
        {
            keyActs.Add(keyword[keyWords], replayAudio);
        } */
        recognizer = new KeywordRecognizer(keyActs.Keys.ToArray());
        recognizer.OnPhraseRecognized += OnKeywordsRecognized;
        recognizer.Start();

        audioSource = this.GetComponent<AudioSource>();

    }

    // Update is called once per frame
    void Update()
    {
        
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
