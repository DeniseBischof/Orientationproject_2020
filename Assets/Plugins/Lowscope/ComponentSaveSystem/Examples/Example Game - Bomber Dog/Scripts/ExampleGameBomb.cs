using UnityEngine;
using System.Collections;
using Lowscope.Saving;
using Lowscope.Saving.Core;
using System;
using UnityEngine.SceneManagement;

namespace Lowscope.Saving.Examples
{
    [AddComponentMenu("")]
    public class ExampleGameBomb : MonoBehaviour, ISaveable
    {
        [SerializeField] private GameObject explosionEffectPrefab;
        [SerializeField] private LayerMask affectedByExplosion;
        [SerializeField] private Transform meshRenderer;

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip audioExplode;

        private float activeDetonationTime = 3;
        private bool hasExploded = false;

        // Update is called once per frame
        void Update()
        {
            if (activeDetonationTime > 0)
            {
                meshRenderer.transform.localScale = Vector3.one + (new Vector3(0.1f, 0.1f, 0.1f) * Mathf.Sin(Time.time * 5));

                activeDetonationTime -= Time.deltaTime;
            }
            else
            {
                if (hasExploded)
                    return;

                Collider[] col = Physics.OverlapSphere(this.transform.position, 2, affectedByExplosion);
                foreach (var item in col)
                {
                    var damageable = item.GetComponent<ExampleGameIDamageable>();
                    if (damageable != null)
                        damageable.Damage(1);
                }

                for (int i = -1; i < 2; i++)
                {
                    GameObject.Instantiate(explosionEffectPrefab, this.transform.position + new Vector3(i * 2f, 0, 0), new Quaternion());
                }

                audioSource.PlayOneShot(audioExplode);

                // Toggle explosioneffect
                GameObject.Destroy(this.gameObject, 1);
                meshRenderer.gameObject.SetActive(false);

                hasExploded = true;
            }
        }

        public void OnLoad(string data)
        {
            bool.TryParse(data, out hasExploded);

            if (hasExploded)
            {
                GameObject.Destroy(this.gameObject);
            }
        }

        public string OnSave()
        {
            return hasExploded.ToString();
        }

        public bool OnSaveCondition()
        {
            return true;
        }
    }
}