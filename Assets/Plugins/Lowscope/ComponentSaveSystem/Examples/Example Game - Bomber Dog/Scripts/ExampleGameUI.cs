using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lowscope.Saving.Examples
{
    [AddComponentMenu("")]
    public class ExampleGameUI : MonoBehaviour
    {
        [SerializeField] private Text textBombCount;
        [SerializeField] private GameObject[] heartIcons;
        [SerializeField] private GameObject[] gemIcons;
        [SerializeField] private ExampleGameObjectPulser bombIcon;

        public void OnBombCountChanged(int newAmount)
        {
            textBombCount.text = newAmount.ToString();
        }

        public void HealthCountChanged(int newAmount)
        {
            int totalIcons = heartIcons.Length;

            for (int i = 0; i < totalIcons; i++)
            {
                heartIcons[i].gameObject.SetActive(i < newAmount);
            }
        }

        public void GemCountChanged(int newAmount)
        {
            int totalIcons = gemIcons.Length;

            for (int i = 0; i < totalIcons; i++)
            {
                gemIcons[i].gameObject.SetActive(i < newAmount);
            }
        }
    }
}