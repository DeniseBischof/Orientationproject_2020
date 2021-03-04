using Lowscope.Saving;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lowscope.Saving.Components
{
    /// <summary>
    /// You can place this component in your active player scene. So when the player
    /// scene gets destroyed/game gets exited, it will remember the last active active scenes in your game.
    /// </summary>
    [AddComponentMenu("Saving/Components/Scene Loading/Save Last Used Scenes"), DisallowMultipleComponent]
    public class SaveLastUsedScenes : MonoBehaviour
    {
        public enum SaveTrigger
        {
            OnSaveWriteToDisk,
            OnSyncSaveDone,
            OnDestroy,
            OnDisable,
            OnEnable,
            Manual
        }

        [SerializeField] private SaveTrigger[] saveTriggers = new SaveTrigger[1] { SaveTrigger.OnSaveWriteToDisk };
        [SerializeField] private bool saveAdditiveScenes = true;

        private HashSet<SaveTrigger> triggers = new HashSet<SaveTrigger>();

        [System.Serializable]
        public class SavedScenes
        {
            public string activeSceneName;
            public List<string> additionalSceneNames;
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
                SaveMaster.OnWritingToDiskBegin += OnEvent;
            }

            if (triggers.Contains(SaveTrigger.OnSyncSaveDone))
            {
                SaveMaster.OnSyncSaveDone += OnEvent;
            }
        }

        private void OnEnable()
        {
            if (triggers.Contains(SaveTrigger.OnEnable))
                Save();
        }

        private void OnDisable()
        {
            if (triggers.Contains(SaveTrigger.OnDisable))
                Save();
        }

        private void OnDestroy()
        {
            if (triggers.Contains(SaveTrigger.OnDestroy))
            {
                Save();
            }

            if (triggers.Contains(SaveTrigger.OnSaveWriteToDisk))
            {
                SaveMaster.OnWritingToDiskBegin -= OnEvent;
            }

            if (triggers.Contains(SaveTrigger.OnSyncSaveDone))
            {
                SaveMaster.OnSyncSaveDone -= OnEvent;
            }
        }

        private void OnEvent(int slot)
        {
            Save();
        }

        public void Save()
        {
            if (SaveMaster.GetActiveSlot() != -1)
            {
                SaveMaster.SetString("LastUsedScenes", JsonUtility.ToJson(GetSavedSceneData()));
            }
        }

        SavedScenes GetSavedSceneData()
        {
            int totalScenes = SceneManager.sceneCount;
            List<string> loadedScenes = new List<string>();

            Scene getActiveScene = SceneManager.GetActiveScene();

            if (saveAdditiveScenes)
            {
                for (int i = 0; i < totalScenes; i++)
                {
                    Scene getAdditionalScene = SceneManager.GetSceneAt(i);

                    if (getActiveScene != getAdditionalScene && getAdditionalScene.isLoaded)
                        loadedScenes.Add(getAdditionalScene.name);
                }
            }

            return new SavedScenes()
            {
                activeSceneName = SceneManager.GetActiveScene().name,
                additionalSceneNames = loadedScenes
            };
        }
    }
}