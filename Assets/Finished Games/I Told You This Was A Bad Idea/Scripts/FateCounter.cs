using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FateCounter : MonoBehaviour
{
    public int Good = 0;
    public int Bad = 0;
    public int Neutral = 0;

    public void AddGood()
    {
        Good = +1;
        Debug.Log("Good: " + Good);
    }
    public void AddBad()
    {
        Bad = +1;
        Debug.Log("Bad: " + Bad);
    }
    public void AddNeutral()
    {
        Neutral = +1;
        Debug.Log("Neutral: " + Neutral);
    }
}
