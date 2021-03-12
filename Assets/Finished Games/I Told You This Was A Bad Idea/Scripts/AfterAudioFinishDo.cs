using System.Collections.Generic;
using UnityEngine.Serialization;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AfterAudioFinishDo : MonoBehaviour
{

    [Serializable]
    public class StartThisEvent : UnityEvent { }

    [FormerlySerializedAs("IfCounterOver")]
    [SerializeField]
    private StartThisEvent getCounter = new StartThisEvent();

    protected AfterAudioFinishDo()
    { }

    public StartThisEvent ifTextFound
    {
        get { return getCounter; }
        set { getCounter = value; }
    }

    private void Press()
    {
        UISystemProfilerApi.AddMarker("Button.onClick", this);
        getCounter.Invoke();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(waitForSound());
    }

    IEnumerator waitForSound()
    {
        while (GetComponent<AudioSource>().isPlaying)
        {
            yield return null;
        }

        Press();


    }
}
