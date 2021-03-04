using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomDetails : MonoBehaviour
{
    [Header("Room Text")]
    [TextArea(3, 50)]
    public string roomText;

    //Turn on interaction when Player is in Room
    void OnTriggerStay(Collider col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            transform.Find("Interaction").gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            transform.Find("Interaction").gameObject.SetActive(false);
        }
    }

}
