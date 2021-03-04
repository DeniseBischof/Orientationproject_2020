using System;
using Lowscope.Saving;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lowscope.Saving.Examples
{
    public class ExampleMainMenu : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button buttonContinue;
        [SerializeField] private Button buttonNew;
        [SerializeField] private Button buttonLoad;
        [SerializeField] private Button buttonQuit;

        [SerializeField] private ExampleErrorScreen errorMessage;
        [SerializeField] private ExampleSlotMenu slotMenu;

        [Header("Configuration")]
        [SerializeField] private string sceneToLoadOnNewGame;

        private int lastUsedValidSlot;

        private void Start()
        {
            lastUsedValidSlot = SaveMaster.GetLastUsedValidSlot();
            buttonContinue.interactable = lastUsedValidSlot != -1;

            buttonContinue.onClick.AddListener(Continue);
            buttonNew.onClick.AddListener(NewGame);
            buttonLoad.onClick.AddListener(LoadGame);
            buttonQuit.onClick.AddListener(QuitGame);

            SaveMaster.OnDeletedSave += OnDeletedSave;
        }

        private void OnDestroy()
        {
            SaveMaster.OnDeletedSave -= OnDeletedSave;
        }

        private void OnDeletedSave(int obj)
        {
            if (lastUsedValidSlot == obj)
                buttonContinue.interactable = false;
        }

        private void Continue()
        {
            SaveMaster.SetSlotToLastUsedSlot(true);
        }

        private void LoadGame()
        {
            slotMenu.gameObject.SetActive(true);
        }

        private void QuitGame()
        {
            Application.Quit();
        }

        private void NewGame()
        {
            int slot;
            if (SaveMaster.SetSlotToNewSlot(false, out slot))
            {
                SceneManager.LoadScene(sceneToLoadOnNewGame);
            }
            else
            {
                errorMessage.Configure("All slots full",
                    "No more available save slots. \n" +
                    "Please overwrite or remove a slot");
            }
        }
    }
}