using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetMousePosition : MonoBehaviour
{
    private Vector3 mousePosition;
    public float moveSpeed = 1f;

    AudioSource wandSound;


    public int counter = 0;
    public int counterForEventSart = 500;

    IEnumerator fadeSound;

    private bool wandMoving;
    public GameObject[] turnOnObject;

    void Start()
    {
        wandSound = GetComponent<AudioSource>();
        //fadeSound = FadeOut(wandSound, 0.5f);
        wandSound.Play(0);
        wandSound.Pause();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        increaseVolumeWithcounter();
        IfWandIsMoving();

        if (Input.GetMouseButton(0))
        {
            wandMoving = true;
            Vector3 temp = Input.mousePosition;
            temp.z = 5f;
            this.transform.position = Camera.main.ScreenToWorldPoint(temp);
            getMouseMovement();

        }
        else
        {
            wandMoving = false;
            counter = 0;
        }


    }

    private void getMouseMovement()
    {
        if (Input.GetAxis("Mouse X") < 0 || Input.GetAxis("Mouse X") > 0)
        {
            wandSound.UnPause();
            counter += 1;
        }

    }

    private void increaseVolumeWithcounter()
    {
        if (counter <= 0)
        {
            StartCoroutine(FadeOut(wandSound, 1f, 0));
        }
        else if(counter >= 30 && counter <= 180)
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

    public void IfWandIsMoving()
    {
        if (wandMoving)
        {
            for (int i = 0; i < turnOnObject.Length; i++)
            {
               
                turnOnObject[i].SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < turnOnObject.Length; i++)
            {
                
                turnOnObject[i].SetActive(false);
            }
        }
    }

    public void checkCounter()
    {
        if(counter > counterForEventSart)
        {

        }
    }
}
