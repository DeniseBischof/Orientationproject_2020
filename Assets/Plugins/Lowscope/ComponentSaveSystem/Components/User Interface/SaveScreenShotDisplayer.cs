using Lowscope.Saving;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lowscope.Saving.Components
{
    [AddComponentMenu("Saving/Components/Extras/Save Screenshot Displayer"), DisallowMultipleComponent]
    public class SaveScreenShotDisplayer : MonoBehaviour
    {
        [SerializeField] private RawImage rawImage;
        [SerializeField] private Texture2D notAvailableImage;
        [SerializeField] private Color availableColor = Color.white;
        [SerializeField] private Color notAvailableColor = new Color(0.7f, 0.7f, 0.7f,1);
        [SerializeField] private bool loadFromSlotAutomatically = false;
        [SerializeField] private int slot = 0;

        private void Reset()
        {
            if (rawImage == null)
                rawImage = GetComponent<RawImage>();
        }

        private void OnEnable()
        {
            if (loadFromSlotAutomatically)
            {
                LoadScreenshot(slot);
            }
        }

        public void LoadScreenshot(int slot)
        {
            if (!SaveMaster.IsSlotUsed(slot))
            {
                rawImage.texture = notAvailableImage;
                rawImage.color = notAvailableColor;
                return;
            }

            int resWidth = Screen.width;
            int resHeight = Screen.height;

            string screenShotWidth = "";

            if (SaveMaster.GetMetaData("screenshot-width", out screenShotWidth, slot))
            {
                int.TryParse(screenShotWidth, out resWidth);
            }

            string screenShotHeight = "";

            if (SaveMaster.GetMetaData("screenshot-height", out screenShotHeight, slot))
            {
                int.TryParse(screenShotHeight, out resHeight);
            }

            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);

            if (SaveMaster.GetMetaData("screenshot", screenShot, slot))
            {
                rawImage.texture = screenShot;
                rawImage.color = availableColor;
            }
            else
            {
                rawImage.texture = notAvailableImage;
                rawImage.color = notAvailableColor;
            }
        }
    }
}