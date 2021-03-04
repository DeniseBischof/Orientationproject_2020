using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lowscope.Saving.Examples
{
    [AddComponentMenu("")]
    public class ExampleGameCamera : MonoBehaviour
    {
        private static ExampleGameCamera instance;

        public static void ShakeCamera()
        {
            if (instance != null)
            {
                instance.activeShake = 0.5f;
            }
        }

        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset;
        [SerializeField] private float speed = 1;

        private float warpDelta = 2.5f;

        private float activeShake = 0;

        private void Awake()
        {
            WarpToPlayer();

            instance = this;
        }

        private void WarpToPlayer()
        {
            this.transform.position = new Vector3()
            {
                x = target.transform.position.x,
                y = offset.y,
                z = offset.z
            };
        }

        void Update()
        {
            Vector3 newPosition = new Vector3()
            {
                x = Mathf.Lerp(transform.position.x, target.transform.position.x, Time.deltaTime * speed) + offset.x,
                y = offset.y,
                z = offset.z
            };

            if (Mathf.Abs(target.transform.position.x - transform.position.x) > warpDelta)
            {
                WarpToPlayer();
            }

            if (activeShake > 0)
            {
                newPosition = newPosition + (Random.insideUnitSphere * activeShake);
                activeShake = Mathf.Clamp(activeShake - Time.deltaTime, 0, 1);
            }

            this.transform.position = new Vector3()
            {
                x = Mathf.Lerp(transform.position.x, target.transform.position.x, Time.deltaTime * speed) + offset.x,
                y = offset.y,
                z = offset.z
            };
        }
    }
}