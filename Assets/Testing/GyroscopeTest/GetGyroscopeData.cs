using UnityEngine;
using System.Collections;
using System.IO.Ports;

public class GetGyroscopeData : MonoBehaviour
{
    SerialPort sp = new SerialPort("COM10", 9600);

    void Start()
    {
        sp.Open();
        sp.DtrEnable = true; // We configure data control by DTR.
        sp.ReadTimeout = 500;
    }

    void Update()
    {

        try
        {
            print(sp.ReadLine());
        }
        catch (System.Exception)
        {
            Debug.Log("Error");
        }

    }
}