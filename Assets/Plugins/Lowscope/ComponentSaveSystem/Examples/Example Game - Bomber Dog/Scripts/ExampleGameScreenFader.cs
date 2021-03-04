using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lowscope.Saving.Examples
{
    [AddComponentMenu("")]
    public class ExampleGameScreenFader : MonoBehaviour
    {
        [SerializeField] private float fadeDuration;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Canvas canvas;

        public void HideScreen()
        {
            canvas.enabled = true;
            StopAllCoroutines();
            StartCoroutine(ChangeOpactiy(1, fadeDuration));
        }

        public void ShowScreen()
        {
            canvas.enabled = true;
            StopAllCoroutines();
            StartCoroutine(ChangeOpactiy(0, fadeDuration));
        }

        private IEnumerator ChangeOpactiy(float target, float duration)
        {
            canvasGroup.alpha = target == 0 ? 1 : 0;
            float a = canvasGroup.alpha;

            float t = 0;

            while (t < duration)
            {
                yield return null;
                canvasGroup.alpha = Mathf.Lerp(a, target, t / duration);
                t += Time.deltaTime;
            }

            canvasGroup.alpha = target;
            if (canvasGroup.alpha == 0)
            {
                canvas.enabled = false;
            }
        }
    }
}
