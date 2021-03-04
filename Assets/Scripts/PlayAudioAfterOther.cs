using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class PlayAudioAfterOther : MonoBehaviour
{
    public AudioClip[] audioClips;
    AudioSource audioKeeper;

    private void Start()
    {
        AudioSource audioKeeper = GetComponent<AudioSource>();
        StartCoroutine(startPlayingSounds());
    }



    void playAudio(int clipNumber)
    {
        audioKeeper.clip = audioClips[clipNumber];
        audioKeeper.Play();
    }

    IEnumerator startPlayingSounds()
    {

        foreach (AudioClip audioClip in audioClips)
        {
            audioKeeper.clip = audioClip;
            audioKeeper.Play();
            yield return new WaitForSeconds(audioKeeper.clip.length);
        }

    }

}