using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_WEBGL
using System.Runtime.InteropServices;
#endif

namespace Lowscope.Saving.Examples
{
    [AddComponentMenu("")]
    public class ExampleGameMainMenu : MonoBehaviour
    {
        [SerializeField] private Button buttonContinue;
        [SerializeField] private Button buttonNew;
        [SerializeField] private Button buttonLoad;
        [SerializeField] private Button buttonAbout;
        [SerializeField] private Button buttonExit;

        [SerializeField] private string startSceneName;

        [SerializeField] private ExampleGameSaveMenu saveLoadMenu;
        [SerializeField] private GameObject creditsMenu;

        [SerializeField] private Button creditsButtonExit;
        [SerializeField] private Button creditsButtonSaveComponent;
        [SerializeField] private Button creditsButtonKenney;
        [SerializeField] private Button creditsButtonKay;

#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void openWindow(string url);
#endif

        private const string urlSaveSystem = "https://assetstore.unity.com/packages/tools/utilities/component-save-system-159191?aid=1101lHUQ";
        private const string urlKenney = "https://kenney.nl";
        private const string urlKay = "https://kaylousberg.itch.io/";

        private int lastUsedSlot;

        private void Awake()
        {
            buttonContinue.onClick.AddListener(OnPressContinue);
            buttonNew.onClick.AddListener(OnPressNewGame);
            buttonLoad.onClick.AddListener(OnPressLoadGame);
            buttonAbout.onClick.AddListener(OnPressAbout);
            buttonExit.onClick.AddListener(OnPressExit);

            creditsButtonExit.onClick.AddListener(() => creditsMenu.gameObject.SetActive(false));
            creditsButtonSaveComponent.onClick.AddListener(() => OpenURL(urlSaveSystem));
            creditsButtonKenney.onClick.AddListener(() => OpenURL(urlKenney));
            creditsButtonKay.onClick.AddListener(() => OpenURL(urlKay));

            lastUsedSlot = SaveMaster.GetLastUsedValidSlot();
            buttonContinue.interactable = lastUsedSlot != -1;
        }

        private void OpenURL(string url)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
        openWindow(url);
#else
            Application.OpenURL(url);
#endif
        }

        private void OnPressContinue()
        {
            SaveMaster.SetSlotToLastUsedSlot(false);
        }

        private void OnPressNewGame()
        {
            SaveMaster.SetSlotToTemporarySlot(false);
            SceneManager.LoadScene(startSceneName);
        }

        private void OnPressLoadGame()
        {
            saveLoadMenu.Display(ExampleGameSaveMenu.Mode.Load);
        }

        private void OnPressAbout()
        {
            saveLoadMenu.Hide();
            creditsMenu.gameObject.SetActive(true);
        }

        private void OnPressExit()
        {
            Application.Quit();
        }
    }
}