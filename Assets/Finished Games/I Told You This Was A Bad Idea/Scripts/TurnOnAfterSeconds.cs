using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOnAfterSeconds : MonoBehaviour
{
    public float sec = 10f;
    public GameObject ObjectToTurnOn;

    public bool turnOnAutomatically = true;
    void Start()
    {
        if(turnOnAutomatically)
        StartCoroutine(LateCall());
    }

    public void turnOn()
    {
        StartCoroutine(LateCall());
    }

    public void turnOff()
    {
        StartCoroutine(LateCallOff());
    }

    IEnumerator LateCall()
    {

        yield return new WaitForSeconds(sec);

        ObjectToTurnOn.SetActive(true);

    }

    IEnumerator LateCallOff()
    {

        yield return new WaitForSeconds(sec);

        ObjectToTurnOn.SetActive(false);

    }
}