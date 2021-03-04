using System.Collections;
using UnityEngine;

namespace Lowscope.Saving.Examples
{
    [AddComponentMenu("")]
    public class ExampleGameButtonWobble : MonoBehaviour
    {
        [SerializeField] private Transform buttonTransform;
        [SerializeField] private float distance = 5;
        [SerializeField] private float frequency = 3;

        private void Reset()
        {
            buttonTransform = this.transform;
        }

        private void OnEnable()
        {
            StartCoroutine(WobbleButton(buttonTransform));
        }

        private IEnumerator WobbleButton(Transform button)
        {
            float customTime = UnityEngine.Random.Range(0f, 1f);

            while (true)
            {
                yield return new WaitForSecondsRealtime(0.01f);
                button.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Cos(customTime * frequency) * distance);
                customTime += 0.01f;
            }
        }
    }
}