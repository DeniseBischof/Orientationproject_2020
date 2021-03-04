using UnityEngine;
using Lowscope.Saving;
using System.Collections.Generic;

namespace Lowscope.Saving.Components
{
    /// <summary>
    /// Writes a screenshot to the metadata of the active save game.
    /// </summary>
    [AddComponentMenu("Saving/Components/Extras/Save Screenshot"), DisallowMultipleComponent]
    public class SaveScreenshot : MonoBehaviour
    {
        public enum SaveTrigger
        {
            OnSaveWriteToDisk,
            OnDestroy,
            OnDisable,
            OnEnable,
            Manual
        }

        [SerializeField] private SaveTrigger[] saveTriggers = new SaveTrigger[1] { SaveTrigger.OnSaveWriteToDisk };

        [SerializeField] private new Camera camera = null;

        [Tooltip("All referenced canvases will be set Screen Space - Camera temporarily. " +
            "Only do this if the canvas isn't getting shown.")]
        [SerializeField] private Canvas[] ensureCanvasDisplay;

        [SerializeField] private GameObject[] hideObjectsFromScreenshot;

        [SerializeField] private LayerMask screenshotCameraCullingMask;

        private RenderMode[] renderModes;

        private HashSet<SaveTrigger> triggers = new HashSet<SaveTrigger>();

        [Tooltip("At what resolution should the screenshots be made?")]
        [SerializeField] private Vector2Int resolution = new Vector2Int(512, 512);

        [Tooltip("How large should the screenshot be based on the current screen resolution?")]
        [SerializeField, Range(0.25f, 1f)] private float resolutionScale = 1;

#if UNITY_EDITOR
        private static int totalComponents = 0;
#endif

        private void Reset()
        {
            camera = Camera.main;

            if (camera != null)
                screenshotCameraCullingMask = camera.cullingMask;
        }

        private void Awake()
        {
            int triggerCount = saveTriggers.Length;
            for (int i = 0; i < triggerCount; i++)
            {
                triggers.Add(saveTriggers[i]);
            }

            if (triggers.Contains(SaveTrigger.OnSaveWriteToDisk))
            {
                SaveMaster.OnWritingToDiskBegin += StoreScreenShot;
            }

#if UNITY_EDITOR
            totalComponents++;
#endif
        }

        private void OnDestroy()
        {
            if (triggers.Contains(SaveTrigger.OnSaveWriteToDisk))
            {
                SaveMaster.OnWritingToDiskBegin -= StoreScreenShot;
            }

            if (triggers.Contains(SaveTrigger.OnDestroy))
            {
                StoreScreenShot();
            }

#if UNITY_EDITOR
            totalComponents--;
#endif
        }

        private void OnEnable()
        {
            if (triggers.Contains(SaveTrigger.OnEnable))
            {
                StoreScreenShot();
            }
        }

        private void OnDisable()
        {
            if (triggers.Contains(SaveTrigger.OnDisable))
            {
                StoreScreenShot();
            }
        }

        public void StoreScreenShot()
        {
            int activeSlot = SaveMaster.GetActiveSlot();

            if (activeSlot == -1)
            {
                Debug.LogError("SaveScreenshot: Attempted to make screenshot, but no slot was loaded.");
                return;
            }

            StoreScreenShot(activeSlot);
        }

        public void StoreScreenShot(int slot)
        {
#if UNITY_EDITOR
            // Simple error message in case you have multiple 
            // save screenshot components in your scene.
            if (totalComponents > 1)
            {
                Debug.Log("Multiple Save Screenshot Components have been triggered. " +
                    "Results may not be what you expect.");
            }
#endif

            if (camera == null)
            {
                Debug.LogError("Failed to create screenshot: No camera reference found");
                return;
            }

            int resWidth = Mathf.CeilToInt(resolution.x * resolutionScale);
            int resHeight = Mathf.CeilToInt(resolution.y * resolutionScale);

            int canvasCount = ensureCanvasDisplay.Length;

            if (renderModes == null)
            {
                renderModes = new RenderMode[canvasCount];
            }

            int hideObjects = hideObjectsFromScreenshot.Length;
            for (int i = 0; i < hideObjects; i++)
            {
                if (hideObjectsFromScreenshot[i] != null)
                {
                    hideObjectsFromScreenshot[i].gameObject.SetActive(false);
                }
            }

            for (int i = 0; i < canvasCount; i++)
            {
                if (ensureCanvasDisplay[i] != null)
                {
                    renderModes[i] = ensureCanvasDisplay[i].renderMode;

                    // Only need to fix the overlay cameras.
                    if (ensureCanvasDisplay[i].renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        ensureCanvasDisplay[i].renderMode = RenderMode.ScreenSpaceCamera;
                        ensureCanvasDisplay[i].worldCamera = camera;
                    }
                }
            }

            int activeCullingMask = camera.cullingMask;
            camera.cullingMask = screenshotCameraCullingMask;

            RenderTexture renderTexture = new RenderTexture(resWidth, resHeight, 24);
            camera.targetTexture = renderTexture;

            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            camera.Render();
            RenderTexture.active = renderTexture;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);

            for (int i = 0; i < canvasCount; i++)
            {
                if (ensureCanvasDisplay[i] != null)
                {
                    ensureCanvasDisplay[i].renderMode = renderModes[i];
                }
            }

            for (int i = 0; i < hideObjects; i++)
            {
                if (hideObjectsFromScreenshot[i] != null)
                {
                    hideObjectsFromScreenshot[i].gameObject.SetActive(true);
                }
            }

            camera.cullingMask = activeCullingMask;
            camera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(renderTexture);

            SaveMaster.SetMetaData("screenshot-width", resWidth.ToString());
            SaveMaster.SetMetaData("screenshot-height", resHeight.ToString());
            SaveMaster.SetMetaData("screenshot", screenShot);
        }
    }
}