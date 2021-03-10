#if UNITY_EDITOR_WIN
using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System;
using System.Linq;

public class GetGyroscopeData : MonoBehaviour
{
    SerialPort sp = new SerialPort("COM10", 9600);
    public Vector3 currentRotation;
    public Vector3 lastRotation = new Vector3(0, 0, 0);
    public float rotationDifference;

    [SerializeField]
    public int counter = 0;

    AudioSource wandSound;

    void Start()
    {
        sp.Open();
        sp.DtrEnable = true; // We configure data control by DTR.
        sp.ReadTimeout = 100; 
        sp.WriteTimeout = 100;

        wandSound = GetComponent<AudioSource>();
        wandSound.Play(0);
        wandSound.Pause();
    }
    void LateUpdate()
    {
        readSerialInput();
        getGyroscopeDifference();
        increaseVolumeWithcounter();

        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.Euler(currentRotation), Time.deltaTime * 2f);
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

    private void readSerialInput()
    {
        //Debug.Log(sp.ReadLine());

        string[] splitResult = sp.ReadLine().Split(',');

        float x = float.Parse(splitResult[0]);
        float y = float.Parse(splitResult[1]);
        float z = float.Parse(splitResult[2]);

        currentRotation = new Vector3(x, y, z);



       // Debug.Log(" 1: " + x + " 2: " + y + " 3: " + z);
    }

    private void getGyroscopeDifference()
    {
        rotationDifference = Vector3.Distance(currentRotation, lastRotation);
        Debug.Log(rotationDifference);

        if (rotationDifference > 25)
        {
            wandSound.UnPause();
            counter += 1;
            lastRotation = currentRotation;
        }
        else
        {
            counter = 0;
        }
    }

    private void increaseVolumeWithcounter()
    {
        if (counter <= 0)
        {
            StartCoroutine(FadeOut(wandSound, 1f, 0));
        }
        else if (counter >= 1 && counter <= 180)
        {
            wandSound.volume = 0.25f;

        }
        else if (counter >= 180 && counter <= 300)
        {
            wandSound.volume = 0.5f;
        }
        else if (counter >= 300 && counter < 500)
        {
            wandSound.volume = 0.75f;
        }
        else if (counter >= 500)
        {
            wandSound.volume = 1f;
        }
    }

    private static IEnumerator FadeOut(AudioSource audioSource, float duration, float targetVolume)
    {
        float currentTime = 0;
        float start = audioSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
        audioSource.Pause();
        yield break;
    }

}
#endif