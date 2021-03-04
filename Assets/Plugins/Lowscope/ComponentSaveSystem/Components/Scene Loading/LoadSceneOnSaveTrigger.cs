using Lowscope.Saving.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lowscope.Saving.Components
{
    [AddComponentMenu("Saving/Components/Scene Loading/Load Scene On Save Trigger"), DisallowMultipleComponent, DefaultExecutionOrder(-9014)]
    public class LoadSceneOnSaveTrigger : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Does not load a scene if the save has just been created.")]
        [SerializeField] private bool ignoreOnNewSave;

        [SerializeField] private LoadTrigger[] loadTriggers = new LoadTrigger[1] { LoadTrigger.OnSlotChanged };

        [SerializeField] private string sceneToLoad;
        [SerializeField] private string[] scenesToLoadAdditive;

        private HashSet<LoadTrigger> triggers = new HashSet<LoadTrigger>();

        private void Awake()
        {
            int totalLoadTriggers = loadTriggers.Length;
            for (int i = 0; i < totalLoadTriggers; i++)
            {
                triggers.Add(loadTriggers[i]);
            }

            if (triggers.Contains(LoadTrigger.OnSlotChanged))
            {
                SaveMaster.OnSlotChangeDone += OnSlotChangeDone;
            }
        }

        private void OnDestroy()
        {
            if (triggers.Contains(LoadTrigger.OnDestroy))
            {
                Load();
            }

            if (triggers.Contains(LoadTrigger.OnSlotChanged))
            {
                SaveMaster.OnSlotChangeDone -= OnSlotChangeDone;
            }
        }

        private void OnDisable()
        {
            if (triggers.Contains(LoadTrigger.OnDisable))
            {
                Load();
            }
        }

        private void OnEnable()
        {
            if (triggers.Contains(LoadTrigger.OnEnable))
            {
                Load();
            }
        }

        private void Start()
        {
            if (triggers.Contains(LoadTrigger.OnStart))
            {
                Load();
            }

            if (triggers.Contains(LoadTrigger.OnSlotChanged))
            {
                var slotChangeInfo = SaveMaster.InitialSlotChangeInfo();

                // Check if first frame
                if (slotChangeInfo.firstFrame && Time.frameCount == 1 && SaveMaster.GetActiveSlot() >= 0)
                {
                    OnSlotChangeDone(slotChangeInfo.toSlot, slotChangeInfo.fromSlot);
                }
            }
        }

        private void OnSlotChangeDone(int newSlot, int fromSlot)
        {
            // Check if slots are valid.
            if (newSlot != -1 && newSlot != -2)
            {
                Load();
            }
        }

        public void Load()
        {
            if (SaveMaster.IsSlotLoaded())
            {
                if (ignoreOnNewSave)
                {
                    if (SaveMaster.IsActiveSaveNewSave())
                    {
                        Debug.Log("Save is new, ignoring");
                        return;
                    }
                }

                SceneManager.LoadScene(sceneToLoad);

                int additiveScenes = scenesToLoadAdditive.Length;
                for (int i = 0; i < additiveScenes; i++)
                {
                    SceneManager.LoadScene(scenesToLoadAdditive[i], LoadSceneMode.Additive);
                }
            }
        }
    }
}
