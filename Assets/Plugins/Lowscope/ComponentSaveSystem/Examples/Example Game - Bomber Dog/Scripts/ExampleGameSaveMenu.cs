using Lowscope.Saving.Data;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lowscope.Saving.Examples
{
    [AddComponentMenu("")]
    public class ExampleGameSaveMenu : MonoBehaviour
    {
        public enum Mode
        {
            None,
            Save,
            Load
        }

        [Header("References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text titleText;
        [SerializeField] private Button switchTabRight;
        [SerializeField] private Button switchTabLeft;
        [SerializeField] private Text tabText;
        [SerializeField] private ExampleGameSaveMenuSlot[] slots;

        [Header("Configuration")]
        [SerializeField] private Mode loadDefaultMode = Mode.None;
        [SerializeField] private float fadeInTime = 0.25f;
        [SerializeField] private float fadeOutTime = 0.25f;

        [Header("Events")]
        public UnityEvent OnSlotLoaded;
        public UnityEvent OnSlotSaved;

        private int totalSlotButtons;
        private int activeTab = 0;
        private int maxTabs;
        private int maxSaveSlotCount = 0;

        private Mode mode;

        private void Awake()
        {
            totalSlotButtons = slots.Length;

            for (int i = 0; i < totalSlotButtons; i++)
            {
                slots[i].Configure(this);
                slots[i].SetSlot(i, true);
                slots[i].LoadScreenshot();
            }

            maxSaveSlotCount = SaveSettings.Get().maxSaveSlotCount;

            if (totalSlotButtons == 0)
                return;

            // Get amount of tabs, subract 1 since we want to count from zero.
            maxTabs = Mathf.CeilToInt(maxSaveSlotCount / totalSlotButtons) - 1;

            if (maxSaveSlotCount < totalSlotButtons)
            {
                switchTabLeft.gameObject.SetActive(false);

                if (switchTabRight != null)
                    switchTabRight.gameObject.SetActive(false);

                if (switchTabLeft != null)
                    switchTabLeft.gameObject.SetActive(false);

                if (tabText != null)
                    tabText.gameObject.SetActive(false);
            }
            else
            {
                if (switchTabRight != null)
                    switchTabRight.onClick.AddListener(() => SwitchTab(1));

                if (switchTabLeft != null)
                    switchTabLeft.onClick.AddListener(() => SwitchTab(-1));

                if (tabText != null)
                    tabText.text = string.Format("{0} / {1}", activeTab + 1, maxTabs + 1);
            }
        }

        private void ResetValues()
        {
            activeTab = 0;
        }

        private void OnDisable()
        {
            canvasGroup.alpha = 0;
        }

        private void OnEnable()
        {
            if (loadDefaultMode != Mode.None)
            {
                Display(loadDefaultMode);
            }
        }

        public void SwitchTab(int direction)
        {
            int newTabIndex = Mathf.Clamp(activeTab + direction, 0, maxTabs);

            if (newTabIndex != activeTab)
            {
                activeTab = newTabIndex;
                ReloadSlots(mode);
            }

            if (switchTabLeft != null)
                switchTabLeft.interactable = newTabIndex > 0;

            if (switchTabRight != null)
                switchTabRight.interactable = activeTab != maxTabs;

            if (tabText != null)
                tabText.text = string.Format("{0} / {1}", activeTab + 1, maxTabs + 1);
        }

        // For Unity Events

        public void DisplayLoadMode()
        {
            Display(Mode.Load);
        }

        public void DisplaySaveMode()
        {
            Display(Mode.Save);
        }

        public void Display(Mode mode)
        {
            StartCoroutine(FadeMenu(0, 1, fadeInTime));
            ResetValues();

            if (mode == this.mode)
                return;

            if (titleText != null)
            {
                titleText.text = mode == Mode.Load ? "Load Game" : "Save Game";
            }

            this.mode = mode;
            SwitchTab(0);
            ReloadSlots(mode);
        }

        private void ReloadSlots(Mode mode)
        {
            for (int i = 0; i < totalSlotButtons; i++)
            {
                int indexWithTabOffset = i + (activeTab * totalSlotButtons);

                bool idIsUsed = SaveMaster.IsSlotUsed(indexWithTabOffset);
                bool isValidId = indexWithTabOffset <= maxSaveSlotCount;

                slots[i].SetSlot(indexWithTabOffset, (idIsUsed || mode == Mode.Save) && isValidId);

                string getMetaData;
                SaveMaster.GetMetaData("timeplayed", out getMetaData, indexWithTabOffset);

                slots[i].SetText(idIsUsed ? getMetaData : "Empty");
                slots[i].LoadScreenshot();
            }
        }

        public void Hide()
        {
            StartCoroutine(FadeMenu(1, 0, fadeOutTime));
        }

        public void OnSelectSlot(int i)
        {
            ApplySlot(i);
        }

        private void ApplySlot(int i)
        {
            int indexWithTabOffset = i + (activeTab * totalSlotButtons);

            if (mode == Mode.Load)
            {
                // Just change the slot if we want to load a game
                SaveMaster.SetSlot(indexWithTabOffset, true);

                OnSlotLoaded.Invoke();
            }
            else
            {
                if (mode == Mode.Save)
                {
                    // Change the slot, keep the save data
                    SaveMaster.SetSlot(indexWithTabOffset, false, keepActiveSaveData: true, writeToDiskAfterChange: true);

                    // Load the screenshot, that was made based on the write to disk event.
                    slots[i].LoadScreenshot();

                    OnSlotSaved.Invoke();

                    ReloadSlots(mode);
                }
            }
        }

        IEnumerator FadeMenu(float from, float target, float duration)
        {
            if (duration <= 0)
            {
                canvasGroup.gameObject.SetActive(target == 1);
                yield break;
            }

            if (target == 1)
            {
                canvasGroup.gameObject.SetActive(true);
            }

            float t = 0;
            canvasGroup.alpha = from;

            while (t < duration)
            {
                yield return new WaitForSecondsRealtime(0.01f);
                t += 0.01f;
                canvasGroup.alpha = Mathf.Lerp(from, target, t / duration);
            }

            if (target == 0)
            {
                canvasGroup.gameObject.SetActive(false);
            }
        }
    }
}