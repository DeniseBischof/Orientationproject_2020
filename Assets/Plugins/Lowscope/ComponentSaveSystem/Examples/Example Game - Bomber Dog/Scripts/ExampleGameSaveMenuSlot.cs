using Lowscope.Saving.Components;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace Lowscope.Saving.Examples
{
    [AddComponentMenu("")]
    public class ExampleGameSaveMenuSlot : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private SaveScreenShotDisplayer screenShotDisplayer;
        [SerializeField] private Text textSlotNumber;
        [SerializeField] private Text textSlotState;
        [SerializeField] private int slotVisualOffset = 1;

        private int slot;
        private bool hasScreenshotDisplayer;
        private bool hasSlotNumberText;

        private ExampleGameSaveMenu saveLoadMenu;

        private void Awake()
        {
            hasScreenshotDisplayer = screenShotDisplayer != null;
            hasSlotNumberText = textSlotNumber != null;

            button.onClick.AddListener(LoadSlot);
        }

        // Only called upon add or reset
        private void Reset()
        {
            if (button == null)
            {
                button = GetComponentInChildren<Button>(true);
            }

            if (textSlotNumber == null)
            {
                textSlotNumber = GetComponentInChildren<Text>(true);
            }

            if (screenShotDisplayer == null)
            {
                screenShotDisplayer = GetComponentInChildren<SaveScreenShotDisplayer>(true);
            }
        }

        private void LoadSlot()
        {
            saveLoadMenu.OnSelectSlot(slot);
        }

        public virtual void Configure(ExampleGameSaveMenu saveLoadMenu)
        {
            this.saveLoadMenu = saveLoadMenu;
        }

        public virtual void SetSlot(int slotIndex, bool isInteractable)
        {
            this.slot = slotIndex;

            button.interactable = isInteractable;

            if (hasSlotNumberText)
            {
                textSlotNumber.text = (slotIndex + slotVisualOffset).ToString();
                textSlotNumber.color = new Color(1, 1, 1, isInteractable ? 1 : 0.5f);
            }
        }

        public void SetText(string text)
        {
            if (textSlotState != null)
                textSlotState.text = text;
        }

        public virtual void LoadScreenshot()
        {
            if (hasScreenshotDisplayer)
            {
                screenShotDisplayer.LoadScreenshot(this.slot);
            }
        }
    }
}