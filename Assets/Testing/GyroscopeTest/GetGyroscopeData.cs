using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System;
using System.Linq;

public class GetGyroscopeData : MonoBehaviour
{
    SerialPort sp = new SerialPort("COM10", 9600);
    public Transform target;
    public Vector3 rotationTest;

    void Start()
    {
        sp.Open();
        sp.DtrEnable = true; // We configure data control by DTR.
        sp.ReadTimeout = 100; 
        sp.WriteTimeout = 100;
    }
    void Update()
    {
         Debug.Log(sp.ReadLine());

        string[] splitResult = sp.ReadLine().Split(',');
        Debug.Log(" 1: " + splitResult[0] + " 2: " + splitResult[1] + " 3: " + splitResult[2]);

    }

    public string CheckForRecievedData()
    {
        try 
        {
            string inData = sp.ReadLine();
            sp.BaseStream.Flush();
            sp.DiscardInBuffer();
            return inData;
        }
        catch { return string.Empty; }
    }

}