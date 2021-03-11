using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotAnswered : MonoBehaviour
{
    public float sec = 10f;

    void Start()
    {
        StartCoroutine(LateCall());
    }

    IEnumerator LateCall()
    {

        yield return new WaitForSeconds(sec);

        gameObject.SetActive(false);

    }
}
