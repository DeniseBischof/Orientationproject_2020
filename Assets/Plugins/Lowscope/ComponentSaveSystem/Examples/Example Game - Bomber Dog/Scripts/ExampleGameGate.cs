using Lowscope.Saving;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lowscope.Saving.Examples
{
    [AddComponentMenu("")]
    public class ExampleGameGate : MonoBehaviour, ExampleGameIInteractable, ISaveable
    {
        [SerializeField] private int gemCount;
        [SerializeField] private GameObject gemVisualizer;
        [SerializeField] private GameObject fence;
        [SerializeField] private ExampleGameWarpPoint warpPoint;

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip audioOpen;

        private bool isOpen;

        public void OnInteract(GameObject interactor)
        {
            if (interactor.CompareTag("Player"))
            {
                if (!isOpen)
                {
                    var player = interactor.GetComponent<ExampleGamePlayer>();

                    if (!player.AdjustGems(-gemCount))
                    {
                        StartCoroutine(ShakeFence());
                        return;
                    }
                    else
                    {
                        audioSource.PlayOneShot(audioOpen);

                        SetOpen();
                    }
                }
                else
                {
                    if (isOpen)
                    {
                        var getPlayer = interactor.GetComponent<ExampleGamePlayer>();
                        getPlayer.MoveToLevel(warpPoint.TargetScene, warpPoint.TargetPosition);
                    }
                }
            }
        }

        private void SetOpen()
        {
            fence.SetActive(false);
            gemVisualizer.SetActive(false);
            isOpen = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                int childCount = gemVisualizer.transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    StartCoroutine(ScaleTransform(gemVisualizer.transform.GetChild(i), Vector3.zero, Vector3.one * 0.5f, 0.15f));
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                int childCount = gemVisualizer.transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    StartCoroutine(ScaleTransform(gemVisualizer.transform.GetChild(i), Vector3.one * 0.5f, Vector3.zero, 0.15f));
                }
            }
        }

        IEnumerator ScaleTransform(Transform transform, Vector3 from, Vector3 to, float duration)
        {
            float t = 0;

            while (t < duration)
            {
                yield return null;
                t += Time.deltaTime;
                transform.localScale = Vector3.Lerp(from, to, t / duration);
            }
        }

        IEnumerator ShakeFence()
        {
            fence.transform.rotation = Quaternion.Euler(0, 0, 0);

            float t = 0;
            float duration = 0.25f;

            while (t < duration)
            {
                yield return null;

                float correctedT = ExampleGameEasings.QuadraticInOut(t / duration);

                Quaternion rot = Quaternion.Euler(0, 0, (Mathf.Sin((correctedT) * (Mathf.PI * 4)) * 2));

                fence.transform.localRotation = rot;

                t += Time.deltaTime;
            }
        }

        public void OnLoad(string data)
        {
            bool result;
            if (bool.TryParse(data, out result))
            {
                if (result)
                {
                    SetOpen();
                }
            }
        }

        public string OnSave()
        {
            return isOpen.ToString();
        }

        public bool OnSaveCondition()
        {
            return isOpen;
        }
    }
}