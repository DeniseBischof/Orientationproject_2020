using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lowscope.Saving.Examples
{
    [AddComponentMenu("")]
    public class ExampleGamePauseMenu : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button buttonContinue;
        [SerializeField] private Button buttonSave;
        [SerializeField] private Button buttonLoad;
        [SerializeField] private Button buttonExit;
        [SerializeField] private Button buttonRestart;
        [SerializeField] private RectTransform screenBottom;
        [SerializeField] private ExampleGameSaveMenu saveLoadMenu;

        [SerializeField] private string mainMenuName = "";
        [SerializeField] private string firstSceneName = "";

        bool canvasEnabled = false;

        private void Awake()
        {
            buttonContinue.onClick.AddListener(Continue);
            buttonSave.onClick.AddListener(OpenSaveMenu);
            buttonLoad.onClick.AddListener(OpenLoadMenu);
            buttonExit.onClick.AddListener(ExitToMainMenu);
            buttonRestart.onClick.AddListener(RestartGame);
            canvas.enabled = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                canvasEnabled = !canvasEnabled;

                if (canvasEnabled)
                {
                    StartCoroutine(MoveUpButton(buttonContinue.transform as RectTransform, 0.35f, 0f));
                    StartCoroutine(MoveUpButton(buttonSave.transform as RectTransform, 0.35f, 0.05f));
                    StartCoroutine(MoveUpButton(buttonLoad.transform as RectTransform, 0.35f, 0.1f));
                    StartCoroutine(MoveUpButton(buttonRestart.transform as RectTransform, 0.35f, 0.15f));
                    StartCoroutine(MoveUpButton(buttonExit.transform as RectTransform, 0.35f, 0.20f));
                }

                if (canvasEnabled)
                {
                    StartCoroutine(FadeCanvas(canvasEnabled ? 1 : 0, 0.2f));
                }
                else
                {
                    canvas.enabled = false;
                }

                Time.timeScale = Time.timeScale == 1 ? 0 : 1;
            }
        }

        private IEnumerator FadeCanvas(float target, float duration)
        {
            if (canvasEnabled)
                canvas.enabled = true;

            float activeAlpha = canvasGroup.alpha;
            float t = 0;

            while (t < duration)
            {
                yield return new WaitForSecondsRealtime(0.01f);
                t += 0.01f;
                canvasGroup.alpha = Mathf.Lerp(activeAlpha, target, t / duration);
            }

            canvasGroup.alpha = target;
            canvas.enabled = canvasEnabled;
        }

        private IEnumerator MoveUpButton(RectTransform button, float duration, float startPause)
        {
            float t = 0;
            Vector2 targetPosition = button.anchoredPosition;
            Vector2 fromPosition = button.anchoredPosition;
            fromPosition.y = screenBottom.localPosition.y;

            button.anchoredPosition = fromPosition;

            yield return new WaitForSecondsRealtime(startPause);

            while (t < duration || !canvasEnabled)
            {
                yield return new WaitForSecondsRealtime(0.003f);
                button.anchoredPosition = Vector2.Lerp(fromPosition, targetPosition, ExampleGameEasings.EaseOutBounce(t / duration));
                t += 0.003f;

                if (!canvasEnabled)
                    break;
            }

            button.anchoredPosition = targetPosition;
        }

        public void ExitToMainMenu()
        {
            SceneManager.LoadScene(mainMenuName);
        }

        private void OpenLoadMenu()
        {
            saveLoadMenu.Display(ExampleGameSaveMenu.Mode.Load);
        }

        private void OpenSaveMenu()
        {
            saveLoadMenu.Display(ExampleGameSaveMenu.Mode.Save);
        }

        public void RestartGame()
        {
            SaveMaster.ClearListeners(false);
            SaveMaster.SetSlotToTemporarySlot(false);
            SceneManager.LoadScene(firstSceneName);
        }

        public void Continue()
        {
            canvas.enabled = false;
            canvasEnabled = false;
            Time.timeScale = 1;
        }

        private void OnDestroy()
        {
            Time.timeScale = 1;
        }
    }
}