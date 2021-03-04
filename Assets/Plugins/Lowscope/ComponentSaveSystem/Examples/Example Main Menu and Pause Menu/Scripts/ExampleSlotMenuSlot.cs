using UnityEngine;
using UnityEngine.UI;
using Lowscope.Saving.Components;
using UnityEngine.EventSystems;

namespace Lowscope.Saving.Examples
{
    public class ExampleSlotMenuSlot : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerExitHandler
    {
        [SerializeField] private Text slotIndicator;
        [SerializeField] private Text slotText;
        [SerializeField] private SaveScreenShotDisplayer screenshotDisplayer;
        [SerializeField] private Button buttonRemoveSave;
        [SerializeField] private ExamplePlaySoundsOnMouse playSoundsOnMouse;

        private ExampleSlotMenu.Mode mode;

        private int slotIndex = -1;
        private bool isSlotUsed = false;
        private bool isSlotValid = false;

        private Color initialSlotIndicatorColor;
        private Color initialSlotTextColor;

        private void Awake()
        {
            initialSlotIndicatorColor = slotIndicator.color;
            initialSlotTextColor = slotText.color;
            buttonRemoveSave.onClick.AddListener(OnClickRemove);

            SaveMaster.OnSlotChangeDone += OnChangedSlot;
        }

        private void OnDestroy()
        {
            SaveMaster.OnSlotChangeDone -= OnChangedSlot;
        }

        private void OnClickRemove()
        {
            if (isSlotUsed && isSlotValid)
            {
                SaveMaster.DeleteSave(slotIndex);
                SetIndex(slotIndex, mode);
            }
        }

        private void OnChangedSlot(int to, int from)
        {
            if (to != slotIndex)
                return;

            // Ensure the slot updates if anything is saved to it.
            if (mode == ExampleSlotMenu.Mode.Save)
                SetIndex(slotIndex, mode);
        }

        private void Reset()
        {
            if (slotIndicator == null)
                slotIndicator = GetComponent<Text>();
        }

        public void SetIndex(int slotIndex, ExampleSlotMenu.Mode mode)
        {
            this.slotIndex = slotIndex;
            this.mode = mode;
            isSlotUsed = SaveMaster.IsSlotUsed(slotIndex);
            isSlotValid = SaveMaster.IsSlotValid(slotIndex);

            slotIndicator.text = string.Format("{0}.", (slotIndex + 1).ToString());

            string lastTimeSaved;
            SaveMaster.GetMetaData("lastsavedtime", out lastTimeSaved, slotIndex);

            buttonRemoveSave.gameObject.SetActive(isSlotUsed && isSlotValid);

            if (isSlotValid)
            {
                playSoundsOnMouse.PlaySounds(isSlotUsed || mode == ExampleSlotMenu.Mode.Save);
                slotText.text = isSlotUsed ? lastTimeSaved : "Slot is empty";
            }
            else
            {
                playSoundsOnMouse.PlaySounds(false);
                slotText.text = "";
                slotIndicator.text = "";
            }
        }

        // Called when clicking on the slot
        public void OnPointerDown(PointerEventData eventData)
        {
            if (mode == ExampleSlotMenu.Mode.Load)
            {
                if (slotIndex == -1 || !isSlotUsed || !isSlotValid)
                    return;

                SaveMaster.SetSlot(slotIndex, true);
            }
            else
            {
                if (!isSlotValid)
                    return;

                // Change the slot, keep the save data
                SaveMaster.SetSlot(slotIndex, false, keepActiveSaveData: true, writeToDiskAfterChange: true);
            }
        }

        // Called when pointer hovers over the slot
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (slotIndex == -1)
                return;

            if (screenshotDisplayer != null)
            {
                screenshotDisplayer.LoadScreenshot(slotIndex);
            }

            if (isSlotUsed || (mode == ExampleSlotMenu.Mode.Save && isSlotValid))
            {
                Hovering(true);
            }
        }

        private void Hovering(bool state)
        {
            slotIndicator.color = state ? Color.white : initialSlotIndicatorColor;
            slotText.color = state ? Color.white : initialSlotTextColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Hovering(false);
        }
    }
}