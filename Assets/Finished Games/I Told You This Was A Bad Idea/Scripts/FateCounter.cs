using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FateCounter : MonoBehaviour
{
    public int Good = 0;
    public int Knowledge = 0;

    public GameObject EndSequence;
    public GameObject GoodEnding;
    public GameObject BadEnding;

    public void AddGood()
    {
        Good = Good+1;
        Debug.Log("Good: " + Good);
    }

    public void AddKnowledge()
    {
        Knowledge = Knowledge + 1;
        Debug.Log("Neutral: " + Knowledge);
    }

    public void CheckKnowledge()
    {
        if (Knowledge > 4)
        {
            EndSequence.SetActive(true);
        }
    }

    public void checkFate()
    {
        if(Good> 4)
        {
            GoodEnding.SetActive(true);
        }
        else
        {
            BadEnding.SetActive(true);
        }

    }
}
