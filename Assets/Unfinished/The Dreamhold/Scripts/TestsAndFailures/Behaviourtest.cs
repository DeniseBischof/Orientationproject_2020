#if UNITY_EDITOR_WIN
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class Behaviourtest : MonoBehaviour
{

    private Dictionary<string, Action> keyActs = new Dictionary<string, Action>();
    private KeywordRecognizer recognizer;
    // Start is called before the first frame update
    void Start()
    {
        keyActs.Add("test", OhNo);
        recognizer = new KeywordRecognizer(keyActs.Keys.ToArray());
        recognizer.OnPhraseRecognized += OnKeywordsRecognized;
        recognizer.Start();

    }
    void OnKeywordsRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log("Command: " + args.text);
        keyActs[args.text].Invoke();
    }

    void OhNo()
    {
        Debug.Log("Damn it!");
        recognizer.Stop();
    }
}
#endif