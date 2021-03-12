using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityWebGLSpeechDetection;

public class TurnOffScriptInAllChildren : MonoBehaviour
{
    public Component[] CommandsTurnOff;

    // Start is called before the first frame update
    void Start()
    {
        CommandsTurnOff = GetComponentsInChildren<CommandTurnOffAfterUse>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TurnOffVoiceRecognition()
    {
        foreach (CommandTurnOffAfterUse ctoau in CommandsTurnOff)
        {
            ctoau.turnOffThisAnswer();
        }
    }

    public void TurnOnVoiceRecognition()
    {
        foreach (CommandTurnOffAfterUse ctoau in CommandsTurnOff)
        {
            ctoau.turnOnThisAnswer();
        }
    }
}
