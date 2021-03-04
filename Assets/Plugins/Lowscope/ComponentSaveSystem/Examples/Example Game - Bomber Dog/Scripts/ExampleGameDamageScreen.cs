using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lowscope.Saving.Examples
{
    [AddComponentMenu("")]
    public class ExampleGameDamageScreen : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private Image image;

        public void DisplayDamage(float duration)
        {
            canvas.enabled = true;
            StartCoroutine(DisplayDamageCoroutine(duration));
        }

        IEnumerator DisplayDamageCoroutine(float duration)
        {
            float t = 0;
            Color activeColor = image.color;

            while (t < duration)
            {
                yield return null;
                t += Time.deltaTime;
                activeColor.a = Mathf.Lerp(1, 0, t / duration);
                image.color = activeColor;
            }

            canvas.enabled = false;
        }
    }
}