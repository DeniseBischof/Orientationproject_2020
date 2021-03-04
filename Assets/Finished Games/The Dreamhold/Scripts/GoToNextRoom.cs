using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class GoToNextRoom : MonoBehaviour
{
    private Dictionary<string, Action> keyActs = new Dictionary<string, Action>();
    private KeywordRecognizer recognizer;

    public enum MoveDirections
    {
        north,
        east,
        south,
        west
    }

    public Animator animator;
    public string animationToPlay;
    public MoveDirections Direction;

    public GameObject roomToTurnOn;
    public GameObject roomToTurnOff;

    private void Awake()
    {

    }
    void Start()
    {
        keyActs.Add(Direction.ToString(), MoveToRoom);
        recognizer = new KeywordRecognizer(keyActs.Keys.ToArray());
        recognizer.OnPhraseRecognized += OnKeywordsRecognized;
        recognizer.Start();
    }

    void OnKeywordsRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log("Command: " + args.text);
        keyActs[args.text].Invoke();
    }

    public void MoveToRoom()
    {
        animator.Play(animationToPlay);
        roomToTurnOn.SetActive(true);
        roomToTurnOff.SetActive(false);
    }

}                     
