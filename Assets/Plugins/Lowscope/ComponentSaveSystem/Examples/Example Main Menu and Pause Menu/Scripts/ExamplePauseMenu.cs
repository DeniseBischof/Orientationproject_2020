using Lowscope.Saving;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lowscope.Saving.Examples
{
    public class ExamplePauseMenu : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject pauseMenuObjects;
        [SerializeField] private ExampleSlotMenu slotMenu;
        [SerializeField] private Button buttonLoad;
        [SerializeField] private Button buttonSave;
        [SerializeField] private Button buttonQuit;

        [Header("Configuration")]
        [SerializeField] private KeyCode[] openMenuKeys = new KeyCode[1] { KeyCode.Escape };
        [SerializeField] private string quitToScene;
        [SerializeField] private bool closeWindowOnSave = true;
        [SerializeField] private bool closeWindowOnLoad = true;
        [SerializeField] private GameObject[] toggleObjectVisibility;

        private Dictionary<GameObject, bool> cachedVisibility = new Dictionary<GameObject, bool>();

        int openMenuKeyCount;

        private void Awake()
        {
            openMenuKeyCount = openMenuKeys.Length;

            buttonLoad.onClick.AddListener(OnOpenSlotMenuLoad);
            buttonSave.onClick.AddListener(OnOpenSlotMenuSave);
            buttonQuit.onClick.AddListener(OnQuit);

            if (closeWindowOnSave)
            {
                SaveMaster.OnWritingToDiskDone += OnWrittenToDisk;
            }

            if (closeWindowOnLoad)
            {
                SaveMaster.OnSlotChangeDone += OnChangedSlot;
            }

            int toggleVisibilityCount = toggleObjectVisibility.Length;
            for (int i = 0; i < toggleVisibilityCount; i++)
            {
                cachedVisibility.Add(toggleObjectVisibility[i], toggleObjectVisibility[i].activeSelf);
            }
        }

        private void OnDestroy()
        {
            if (closeWindowOnSave)
            {
                SaveMaster.OnWritingToDiskDone -= OnWrittenToDisk;
            }

            if (closeWindowOnLoad)
            {
                SaveMaster.OnSlotChangeDone -= OnChangedSlot;
            }
        }

        private void OnChangedSlot(int arg1, int arg2)
        {
            Hide();
        }

        private void OnWrittenToDisk(int obj)
        {
            Hide();
        }

        private void OnEnable()
        {
            Hide();
        }

        private void OnQuit()
        {
            SceneManager.LoadScene(quitToScene);
        }

        private void OnOpenSlotMenuSave()
        {
            slotMenu.SetMode(ExampleSlotMenu.Mode.Save);
            slotMenu.gameObject.SetActive(true);
        }

        private void OnOpenSlotMenuLoad()
        {
            slotMenu.SetMode(ExampleSlotMenu.Mode.Load);
            slotMenu.gameObject.SetActive(true);
        }

        public void Display()
        {
            slotMenu.gameObject.SetActive(false);
            pauseMenuObjects.gameObject.SetActive(true);

            int toggleVisibilityCount = toggleObjectVisibility.Length;
            for (int i = 0; i < toggleVisibilityCount; i++)
            {
                toggleObjectVisibility[i].gameObject.SetActive(false);
            }
        }

        public void Hide()
        {
            slotMenu.gameObject.SetActive(false);
            pauseMenuObjects.gameObject.SetActive(false);

            int toggleVisibilityCount = toggleObjectVisibility.Length;
            for (int i = 0; i < toggleVisibilityCount; i++)
            {
                bool visible;
                if (cachedVisibility.TryGetValue(toggleObjectVisibility[i], out visible))
                {
                    toggleObjectVisibility[i].gameObject.SetActive(visible);
                }
            }
        }

        private void Update()
        {
            for (int i = 0; i < openMenuKeyCount; i++)
            {
                if (Input.GetKeyDown(openMenuKeys[i]))
                {
                    if (!pauseMenuObjects.activeSelf)
                    {
                        Display();
                    }
                    else
                    {
                        Hide();
                    }
                    return;
                }
            }
        }
    }
}