using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lowscope.Saving.Examples
{
    public class ExamplePlaySoundsOnMouse : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip soundHover;
        [SerializeField] private AudioClip soundInteract;
        [SerializeField] private Button button;

        private bool playSounds = true;

        public void PlaySounds(bool playSounds)
        {
            this.playSounds = playSounds;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!playSounds)
                return;

            if (button != null)
                if (!button.interactable)
                    return;

            if (audioSource != null && soundInteract != null)
                audioSource.PlayOneShot(soundInteract);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!playSounds)
                return;

            if (button != null)
                if (!button.interactable)
                    return;

            if (audioSource != null && soundHover != null)
                audioSource.PlayOneShot(soundHover);
        }
    }
}