using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Lowscope.Saving;
using Lowscope.Saving.Data;

namespace Lowscope.Saving.Examples
{
    public class ExampleSlotMenu : MonoBehaviour
    {
        public enum Mode
        {
            Load,
            Save
        }

        [Header("References")]
        [SerializeField] private ExampleSlotMenuSlot[] slots;
        [SerializeField] private Text textTab;
        [SerializeField] private Button switchTabLeft;
        [SerializeField] private Button switchTabRight;
        [SerializeField] private Button buttonClose;
        [SerializeField] private Text titleText;

        [Header("Configuration")]
        [SerializeField] private bool hideSwitchTabWhenUnusable = true;
        [SerializeField] private Mode mode = Mode.Load;
        [SerializeField] private string titleTextLoad;
        [SerializeField] private string titleTextSave;

        int slotCount = 0;
        int activeTab = 0;
        int totalTabCount;

        private void Awake()
        {
            slotCount = slots.Length;

            if (slotCount == 0)
                return;

            totalTabCount = Mathf.CeilToInt((float)SaveSettings.Get().maxSaveSlotCount / (float)slotCount) - 1;

            switchTabRight.onClick.AddListener(() => SwitchTab(1));
            switchTabLeft.onClick.AddListener(() => SwitchTab(-1));

            UpdateSlots();

            buttonClose.onClick.AddListener(() => this.gameObject.SetActive(false));

            SwitchTab(0);
        }

        public void SetMode(Mode mode)
        {
            activeTab = 0;
            this.mode = mode;
            UpdateSlots();
        }

        private void UpdateSlots()
        {
            titleText.text = mode == Mode.Load ? titleTextLoad : titleTextSave;

            for (int i = 0; i < slotCount; i++)
            {
                slots[i].SetIndex(i + (activeTab * slotCount), mode);
            }

            if (hideSwitchTabWhenUnusable)
            {
                bool displayTabSwitcher = SaveMaster.IsSlotValid(slotCount);

                switchTabLeft.gameObject.SetActive(displayTabSwitcher);
                switchTabRight.gameObject.SetActive(displayTabSwitcher);
                textTab.gameObject.SetActive(displayTabSwitcher);
            }
        }

        private void SwitchTab(int direction)
        {
            activeTab = Mathf.Clamp(activeTab + direction, 0, totalTabCount);
            switchTabLeft.interactable = activeTab > 0;
            switchTabRight.interactable = activeTab < totalTabCount;
            UpdateTextTab();
            UpdateSlots();
        }

        private void UpdateTextTab()
        {
            textTab.text = string.Format("{0} / {1}", activeTab + 1, totalTabCount + 1);
        }
    }
}