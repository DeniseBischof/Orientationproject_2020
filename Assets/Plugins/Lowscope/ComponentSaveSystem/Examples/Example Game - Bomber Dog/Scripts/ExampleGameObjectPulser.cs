using UnityEngine;

namespace Lowscope.Saving.Examples
{
    [AddComponentMenu("")]
    public class ExampleGameObjectPulser : MonoBehaviour
    {
        [SerializeField] private bool applyRotation;
        [SerializeField] private Vector3 rotationRange = new Vector3(5, 5, 5);
        [SerializeField] private float modulation = 1;
        [SerializeField] private Transform mesh;

        [SerializeField] private bool hoverHeight;
        [SerializeField] private float hoverHeightDistance;
        [SerializeField] private float hoverHeightModulation;
        [SerializeField] private Vector3 hoverHeightOffset;

        Vector3 timeOffset = new Vector3();
        Vector3 velocityOffset = new Vector3();

        private void Awake()
        {
            for (int i = 0; i < 3; i++)
            {
                timeOffset[i] += Random.Range(0f, 3f);
                velocityOffset[i] = Random.Range(0.95f, 1.55f);
            }
        }

        private void Update()
        {
            Vector3 randomRot = new Vector3()
            {
                x = (Mathf.Sin(Time.time * velocityOffset[0] * modulation) * timeOffset[0]) * rotationRange[0],
                y = (Mathf.Sin(Time.time * velocityOffset[1] * modulation) * timeOffset[1]) * rotationRange[1],
                z = (Mathf.Sin(Time.time * velocityOffset[2] * modulation) * timeOffset[2]) * rotationRange[2]
            };

            if (applyRotation)
                mesh.transform.rotation = Quaternion.Euler(randomRot);

            if (hoverHeight)
                mesh.transform.localPosition = new Vector3(0, Mathf.Sin(Time.time * hoverHeightModulation) * hoverHeightDistance, 0) + hoverHeightOffset;
        }
    }
}
