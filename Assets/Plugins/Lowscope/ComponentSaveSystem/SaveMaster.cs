using System;
using System.Collections;
using System.Collections.Generic;
using Lowscope.Saving.Components;
using Lowscope.Saving.Core;
using Lowscope.Saving.Data;
using Lowscope.Saving.Enums;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lowscope.Saving
{
    /// <summary>
    /// Responsible for notifying all Saveable components
    /// Asking them to send data or retrieve data from/to the SaveGame
    /// </summary>
    [AddComponentMenu(""), DefaultExecutionOrder(-9015)]
    public class SaveMaster : MonoBehaviour
    {
        private static SaveMaster instance;

        // Used to track duplicate scenes.
        private static Dictionary<string, int> loadedSceneNames = new Dictionary<string, int>();
        private static HashSet<int> duplicatedSceneHandles = new HashSet<int>();

        private static Dictionary<int, SaveInstanceManager> saveInstanceManagers
            = new Dictionary<int, SaveInstanceManager>();

        private static bool isQuittingGame;

        // Active save game data
        private static SaveGame activeSaveGame = null;
        private static int activeSlot = -1;

        private static bool invokedWritingToDiskEvent = false;

        internal struct SlotChangeInfo
        {
            public int fromSlot;
            public int toSlot;
            public bool firstFrame;
        }

        // All listeners
        private static HashSet<Saveable> saveables = new HashSet<Saveable>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateInstance()
        {
            // Clear everything in case domain reload is disabled.
            instance = null;
            saveInstanceManagers.Clear();
            loadedSceneNames.Clear();
            duplicatedSceneHandles.Clear();
            saveables.Clear();
            customPrefabSpawners.Clear();
            activeSaveGame = null;
            invokedWritingToDiskEvent = false;
            isQuittingGame = false;
            activeSlot = -1;

            //// Add default prefab loader for resources
            //cachedPrefabLoaders.Add("Resources", (id) => { return Resources.Load(id) as GameObject; });

            GameObject saveMasterObject = new GameObject("Save Master");
            saveMasterObject.AddComponent<SaveMaster>();

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            GameObject.DontDestroyOnLoad(saveMasterObject);
        }

        private static SlotChangeInfo initialSlotChangeInfo;

        /// <summary>
        /// Niche usecase for LoadLastUsedScenes component.
        /// Unable to listen to slot changes upon startup due to execution order
        /// So this is a way around it.
        /// </summary>
        /// <returns></returns>
        internal static SlotChangeInfo InitialSlotChangeInfo()
        {
            return initialSlotChangeInfo;
        }

        /// <summary>
        /// Prefab resource loaders
        /// </summary>

        private static Dictionary<string, Func<string, GameObject>> customPrefabSpawners
            = new Dictionary<string, Func<string, GameObject>>();

        public static void AddPrefabResourceLocation(string id, Func<string, GameObject> function)
        {
            if (!customPrefabSpawners.ContainsKey(id))
                customPrefabSpawners.Add(id, function);
        }

        public static void RemovePrefabResourceLocation(string id)
        {
            if (customPrefabSpawners.ContainsKey(id))
                customPrefabSpawners.Remove(id);
        }

        internal static GameObject GetPrefabResource(string customSourceID, string id)
        {
            Func<string, GameObject> s;
            if (customPrefabSpawners.TryGetValue(customSourceID, out s))
            {
                return s.Invoke(id);
            }
            else
            {
                Debug.Log(string.Format("Could not find prefab spawner: {0}", customSourceID));
                return null;
            }
        }

        /*  
        *  Instance managers exist to keep track of spawned objects.
        *  These managers make it possible to drop a coin, and when you reload the game
        *  the coin will still be there.
        */

        private static void OnSceneUnloaded(Scene scene)
        {
            // If it is a duplicate scene, we just remove this handle.
            if (duplicatedSceneHandles.Contains(scene.GetHashCode()))
            {
                duplicatedSceneHandles.Remove(scene.GetHashCode());
            }
            else
            {
                if (loadedSceneNames.ContainsKey(scene.name))
                {
                    loadedSceneNames.Remove(scene.name);
                }
            }

            if (activeSaveGame == null)
                return;

            SaveInstanceManager instanceManager;
            if (saveInstanceManagers.TryGetValue(scene.GetHashCode(), out instanceManager))
            {
                if (activeSaveGame != null)
                {
                    instanceManager.OnSave(activeSaveGame);
                }
                saveInstanceManagers.Remove(scene.GetHashCode());
            }
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
        {
            // Store a refeference to a non-duplicate scene
            if (!loadedSceneNames.ContainsKey(scene.name))
            {
                loadedSceneNames.Add(scene.name, scene.GetHashCode());
            }
            else
            {
                // These scenes are marked as duplicates. They need special treatment for saving.
                duplicatedSceneHandles.Add(scene.GetHashCode());
            }

            if (activeSaveGame == null)
                return;

            // Dont create save instance manager if there are no saved instances in the scene.
            if (string.IsNullOrEmpty(activeSaveGame.Get(string.Format("SaveMaster-{0}-IM", scene.name))))
            {
                return;
            }

            if (!saveInstanceManagers.ContainsKey(scene.GetHashCode()))
            {
                SpawnInstanceManager(scene);
            }
        }

        /// <summary>
        /// You only need to call this for scenes with a duplicate name. If you have a duplicate ID, you can then 
        /// assign a ID to it. And it will save the data of the saveable to that ID instead.
        /// </summary>
        /// <param name="scene">  </param>
        /// <param name="id"> Add a extra indentification for the scene. Useful for duplicated scenes. </param>
        /// <returns></returns>
        public static SaveInstanceManager SpawnInstanceManager(Scene scene, string id = "")
        {
            // Safety precautions.
            if (!string.IsNullOrEmpty(id) && duplicatedSceneHandles.Contains(scene.GetHashCode()))
            {
                duplicatedSceneHandles.Remove(scene.GetHashCode());
            }

            // Already exists
            if (saveInstanceManagers.ContainsKey(scene.GetHashCode()))
            {
                return null;
            }

            // We spawn a game object seperately, so we can keep it disabled during configuration.
            // This prevents any UnityEngine calls such as Awake or Start
            var go = new GameObject("Save Instance Manager");
            go.gameObject.SetActive(false);

            var instanceManager = go.AddComponent<SaveInstanceManager>();

            SceneManager.MoveGameObjectToScene(go, scene);

            string sceneID = string.IsNullOrEmpty(id) ? scene.name : string.Format("{0}-{1}", scene.name, id);

            saveInstanceManagers.Add(scene.GetHashCode(), instanceManager);

            var saveID = string.Format("{0}-{1}-IM", "SaveMaster", sceneID);

            instanceManager.SaveID = saveID;
            instanceManager.SceneID = sceneID;
            instanceManager.OnLoad(activeSaveGame);

            go.gameObject.SetActive(true);
            return instanceManager;
        }

        /// <summary>
        /// Returns if the object has been destroyed using GameObject.Destroy(obj).
        /// Will return false if it has been destroyed due to the game exitting or scene unloading.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool DeactivatedObjectExplicitly(GameObject gameObject)
        {
            return gameObject.scene.isLoaded && !SaveMaster.isQuittingGame;
        }

        /// <summary>
        /// Returns the active slot. -1 means no slot is loaded
        /// </summary>
        /// <returns> Active slot </returns>
        public static int GetActiveSlot()
        {
            return activeSlot;
        }

        /// <summary>
        /// Returns if any slot is loaded
        /// </summary>
        /// <returns></returns>
        public static bool IsSlotLoaded()
        {
            return !(activeSlot == -1 || activeSaveGame == null);
        }

        /// <summary>
        /// Checks if there are any unused save slots.
        /// </summary>
        /// <returns></returns>
        public static bool HasUnusedSlots()
        {
            return SaveFileUtility.GetAvailableSaveSlot() != -1;
        }

        public static int[] GetUsedSlots()
        {
            return SaveFileUtility.GetUsedSlots();
        }

        public static bool IsSlotUsed(int slot)
        {
            return SaveFileUtility.IsSlotUsed(slot);
        }

        /// <summary>
        /// Returns if the slot exceeds the max slot capacity
        /// </summary>
        /// <returns></returns>
        public static bool IsSlotValid(int slot)
        {
            return slot + 1 <= SaveSettings.Get().maxSaveSlotCount;
        }

        /// <summary>
        /// Reloads the active save file without saving it. Useful if you have a save point system.
        /// </summary>
        public static void ReloadActiveSaveFromDisk()
        {
            int activeSlot = GetActiveSlot();
            ClearSlot(false);
            SetSlot(activeSlot, true);
        }

        /// <summary>
        /// Returns the last used slot. Defaults to -1 if no slot is found, or if it is no longer valid.
        /// </summary>
        /// <returns></returns>
        public static int GetLastUsedValidSlot()
        {
            int lastUsedSlot = PlayerPrefs.GetInt("SM-LastUsedSlot", -1);

            if (lastUsedSlot < 0)
                return -1;

            bool slotValid = SaveFileUtility.IsSlotUsed(lastUsedSlot);

            if (!slotValid)
                return -1;

            return lastUsedSlot;
        }

        /// <summary>
        /// Tries to set the current slot to the last used one.
        /// </summary>
        /// <param name="notifyListeners"> Should a load event be send to all active Saveables?</param>
        /// <returns> If it was able to set the slot to the last used one </returns>
        public static bool SetSlotToLastUsedSlot(bool notifyListeners)
        {
            int lastUsedSlot = GetLastUsedValidSlot();

            if (lastUsedSlot == -1)
            {
                return false;
            }
            else
            {
                SetSlot(lastUsedSlot, notifyListeners);
                return true;
            }
        }

        /// <summary>
        /// Attempts to set the slot to the first unused slot. Useful for creating a new game.
        /// </summary>
        /// <param name="notifyListeners"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        public static bool SetSlotToNewSlot(bool notifyListeners, out int slot)
        {
            int availableSlot = SaveFileUtility.GetAvailableSaveSlot();

            if (availableSlot == -1)
            {
                slot = -1;
                return false;
            }
            else
            {
                SetSlot(availableSlot, notifyListeners);
                slot = availableSlot;
                return true;
            }
        }

        /// <summary>
        /// Ensure save master has not been set to any slot
        /// </summary>
        public static void ClearSlot(bool clearAllListeners = true, bool syncSave = true)
        {
            if (clearAllListeners)
            {
                ClearListeners(syncSave);
            }

            activeSlot = -1;
            activeSaveGame.Dispose();
            activeSaveGame = null;
        }

        /// <summary>
        /// Sets the slot, but does not save the data in the previous slot. This is useful if you want to
        /// save the active game to a new save slot. Like in older games such as Half-Life.
        /// </summary>
        /// <param name="slot"> Slot to switch towards, and copy the current save to </param>
        /// <param name="saveGame"> Set this if you want to overwrite a specific save file </param>
        public static void SetSlotWithoutSave(int slot)
        {
            OnSlotChangeBegin.Invoke(slot, activeSlot);

            int fromSlot = activeSlot;
            activeSlot = slot;
            activeSaveGame = SaveFileUtility.LoadSave(slot, true);

            SyncReset();
            SyncSave();

            OnSlotChangeDone.Invoke(slot, fromSlot);
        }

        public static void SetSlotToTemporarySlot(bool reloadSaveables, bool keepActiveSaveData = false)
        {
            SetSlot(-2, reloadSaveables, keepActiveSaveData: keepActiveSaveData);
        }

        public static TimeSpan? GetTimeSinceLastSave()
        {
            if (activeSaveGame != null && activeSlot != -1)
            {
                // Check if the save file has been saved at all. 
                return DateTime.Now - activeSaveGame.modificationDate;
            }
            else
            {
                Debug.Log("SaveMaster: Could not check active time since last save, since no save was loaded.");
                return null;
            }
        }

        public static bool IsActiveSaveNewSave()
        {
            if (activeSaveGame != null && activeSlot != -1)
            {
                // Check if the save file has been saved at all. 
                return activeSaveGame.creationDate == default(DateTime);
            }
            else
            {
                Debug.Log("SaveMaster: Could not check active save if it was new, since no save was loaded.");
                return false;
            }
        }

        /// <summary>
        /// Set the active save slot. (Do note: If you don't want to auto save on slot switch, you can change this in the save setttings)
        /// </summary>
        /// <param name="slot"> Target save slot </param>
        /// <param name="reloadSaveables"> Send a message to all saveables to load the new save file </param>
        public static void SetSlot(int slot, bool reloadSaveables, SaveGame saveGame = null, bool keepActiveSaveData = false, bool writeToDiskAfterChange = false)
        {
            if (keepActiveSaveData && activeSaveGame == null)
            {
                Debug.Log("SaveMaster: No save slot loaded, unable to keep save data");
                return;
            }

            if (!keepActiveSaveData)
            {
                // Ensure the current game is saved, and write it to disk, if that is wanted behaviour.
                if (SaveSettings.Get().autoSaveOnSlotSwitch && activeSaveGame != null)
                {
                    WriteActiveSaveToDisk();
                }

                if (SaveSettings.Get().cleanSavedPrefabsOnSlotSwitch)
                {
                    ClearActiveSavedPrefabs();
                }
            }

            if ((slot < 0 && slot != -2) || slot > SaveSettings.Get().maxSaveSlotCount)
            {
                Debug.LogWarning("SaveMaster: Attempted to set illegal slot.");
                return;
            }

            OnSlotChangeBegin.Invoke(slot, activeSlot);

            int fromSlot = activeSlot;
            activeSlot = slot;

            // Used by LoadLastUsedScenes component to track if 
            initialSlotChangeInfo = new SlotChangeInfo()
            {
                firstFrame = Time.frameCount == 0,
                fromSlot = fromSlot,
                toSlot = slot
            };

            if (!keepActiveSaveData)
            {
                if (activeSaveGame != null)
                    activeSaveGame.Dispose();

                // If slot is not a temporary slot
                if (slot != -2)
                {
                    activeSaveGame = (saveGame == null) ? SaveFileUtility.LoadSave(slot, true) : saveGame;
                }
                else
                {
                    // Create a temporary save file
                    activeSaveGame = SaveFileUtility.CreateSaveGameInstance(SaveSettings.Get().storageType);
                }

                if (reloadSaveables)
                {
                    SyncLoad();
                }

                SyncReset();
            }
            else
            {
                activeSaveGame.SetFileName(SaveFileUtility.ObtainSlotFileName(slot));
            }

            if (writeToDiskAfterChange)
            {
                WriteActiveSaveToDisk();
            }

            if (slot >= 0)
            {
                PlayerPrefs.SetInt("SM-LastUsedSlot", slot);
            }

            OnSlotChangeDone.Invoke(slot, fromSlot);
        }

        public static DateTime GetSaveCreationTime(int slot)
        {
            if (slot == activeSlot)
            {
                return activeSaveGame.creationDate;
            }

            if (!IsSlotUsed(slot))
            {
                return new DateTime();
            }

            return GetSave(slot, true).creationDate;
        }

        public static DateTime GetSaveCreationTime()
        {
            return GetSaveCreationTime(activeSlot);
        }

        public static TimeSpan GetSaveTimePlayed(int slot)
        {
            if (slot == activeSlot)
            {
                return activeSaveGame.timePlayed;
            }

            if (!IsSlotUsed(slot))
            {
                return new TimeSpan();
            }

            return GetSave(slot, true).timePlayed;
        }

        public static TimeSpan GetSaveTimePlayed()
        {
            return GetSaveTimePlayed(activeSlot);
        }

        public static int GetSaveVersion(int slot)
        {
            if (slot == activeSlot)
            {
                return activeSaveGame.gameVersion;
            }

            if (!IsSlotUsed(slot))
            {
                return -1;
            }

            return GetSave(slot, true).gameVersion;
        }

        public static int GetSaveVersion()
        {
            return GetSaveVersion(activeSlot);
        }

        private static SaveGame GetSave(int slot, bool createIfEmpty = true)
        {
            if (slot == activeSlot)
            {
                return activeSaveGame;
            }

            return SaveFileUtility.LoadSave(slot, createIfEmpty);
        }

        /// <summary>
        /// Automatically done on application quit or pause.
        /// Exposed in case you still want to manually write the active save.
        /// </summary>
        public static void WriteActiveSaveToDisk(bool syncActiveSaveables = true)
        {
            // Dont save if the save game is a temporary slot
            if (activeSlot == -2)
                return;

            if (activeSaveGame != null)
            {
                if (!invokedWritingToDiskEvent)
                {
                    OnWritingToDiskBegin.Invoke(activeSlot);
                    invokedWritingToDiskEvent = true;
                }

                if (syncActiveSaveables)
                {
                    SyncSave();
                }

                SetMetaData("timeplayed", activeSaveGame.timePlayed.ToString(), activeSlot);
                SetMetaData("version", activeSaveGame.gameVersion.ToString(), activeSlot);
                SetMetaData("creationdate", activeSaveGame.creationDate.ToString(), activeSlot);
                SetMetaData("lastsavedtime", DateTime.Now.ToString(), activeSlot);

                SaveFileUtility.WriteSave(activeSaveGame, activeSlot);

                OnWritingToDiskDone.Invoke(activeSlot);
                invokedWritingToDiskEvent = false;
            }
            else
            {
                var settings = SaveSettings.Get();

                if (Time.frameCount != 0 && !(settings.slotLoadBehaviour != SaveSettings.SlotLoadBehaviour.DontLoadSlot && settings.autoSaveOnExit))
                {
                    Debug.Log("Savemaster: Attempted to save with no save game loaded.");
                }
            }
        }

        /// <summary>
        /// Wipe all data of a specified scene. This is useful if you want to reset the saved state of a specific scene.
        /// Use clearSceneSaveables = true, in case you want to clear it before switching scenes.
        /// </summary>
        /// <param name="name"> Name of the scene </param>
        /// <param name="clearSceneSaveables"> Scan and wipe for any saveable in the scene? Else they might save again upon destruction.
        /// You can leave this off for performance if you are certain no active saveables are in the scene.</param>
        public static void WipeSceneData(string name, bool clearSceneSaveables = true)
        {
            if (activeSaveGame == null)
            {
                Debug.LogError("Failed to wipe scene data: No save game loaded.");
                return;
            }

            if (clearSceneSaveables)
            {
                foreach (var saveable in saveables)
                {
                    if (saveable.gameObject.scene.name == name)
                    {
                        saveable.WipeData(activeSaveGame);
                    }
                }
            }

            activeSaveGame.WipeSceneData(name);
        }

        /// <summary>
        /// Wipe all data of a specified saveable
        /// </summary>
        /// <param name="saveable"></param>
        public static void WipeSaveable(Saveable saveable)
        {
            if (activeSaveGame == null)
            {
                return;
            }

            saveable.WipeData(activeSaveGame);
        }

        /// <summary>
        /// Clears all saveable components that are listening to the Save Master
        /// </summary>
        /// <param name="syncSave"></param>
        public static void ClearListeners(bool syncSave)
        {
            if (syncSave && activeSaveGame != null)
            {
                foreach (var saveable in saveables)
                {
                    saveable.OnSaveRequest(activeSaveGame);
                }
            }

            saveables.Clear();
        }

        /// <summary>
        /// Manual function for saving a saveable.
        /// Useful if you have a saveable set to manual saving.
        /// </summary>
        /// <param name="saveable"></param>
        public static void SaveListener(Saveable saveable)
        {
            if (saveable != null && activeSaveGame != null)
            {
                saveable.OnSaveRequest(activeSaveGame);
            }
        }

        /// <summary>
        /// Manual function for loading a saveable.
        /// Useful if you have a saveable set to manual loading
        /// </summary>
        /// <param name="saveable"></param>
        public static void LoadListener(Saveable saveable)
        {
            if (saveable != null && activeSaveGame != null)
            {
                saveable.OnLoadRequest(activeSaveGame);
            }
        }

        /// <summary>
        /// Useful in case components have been added to a saveable.
        /// </summary>
        /// <param name="saveable"></param>
        public static void ReloadListener(Saveable saveable)
        {
            if (saveable != null && activeSaveGame != null)
            {
                saveable.ResetState();
                saveable.OnLoadRequest(activeSaveGame);
            }
        }

        /// <summary>
        /// Add saveable from the notification list. So it can recieve load/save requests.
        /// </summary>
        /// <param name="saveable"> Reference to the saveable that listens to the Save Master </param>
        public static void AddListener(Saveable saveable)
        {
            if (saveable != null && activeSaveGame != null)
            {
                saveable.OnLoadRequest(activeSaveGame);
            }

            saveables.Add(saveable);
        }

        /// <summary>
        /// Add saveable from the notification list. So it can recieve load/save requests.
        /// </summary>
        /// <param name="saveable"> Reference to the saveable that listens to the Save Master </param>
        public static void AddListener(Saveable saveable, bool loadData)
        {
            if (loadData)
            {
                AddListener(saveable);
            }
            else
            {
                saveables.Add(saveable);
            }
        }

        /// <summary>
        /// Remove saveable from the notification list. So it no longers recieves load/save requests.
        /// </summary>
        /// <param name="saveable"> Reference to the saveable that listens to the Save Master </param>
        public static void RemoveListener(Saveable saveable)
        {
            if (saveables.Remove(saveable))
            {
                if (saveable != null && activeSaveGame != null)
                {
                    saveable.OnSaveRequest(activeSaveGame);
                }
            }

            // Ensure everything is saved before writing to disk and disposing it.
            if (isQuittingGame)
            {
                if (!SaveSettings.Get().autoSaveOnExit)
                    return;

                if (saveables.Count == 0 && activeSaveGame != null)
                {
                    WriteActiveSaveToDisk();
                    activeSaveGame.Dispose();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="saveable"> Reference to the saveable that listens to the Save Master </param>
        /// <param name="saveData"> Should it try to save the saveable data to the save file when being removed? </param>
        public static void RemoveListener(Saveable saveable, bool saveData)
        {
            if (saveData)
            {
                RemoveListener(saveable);
            }
            else
            {
                saveables.Remove(saveable);
            }
        }

        /// <summary>
        /// <para>Clears all the existing data. Keep in mind that any existing listeners will still save it's data.</para>
        /// <para>You can set removeListeners to true and reload the scene if you want to reset the data from the active scene.</para>
        /// You can also use reloadActiveScenes if you want to automatically reload all the scenes. Note that this may not work depending on your
        /// project structure.
        /// </summary>
        /// <param name="removeListeners"> Ensures no listeners (saveables) can save to the new save file. </param>
        /// <param name="reloadActiveScenes"> Fetches the active saved scene and any additional scenes and spawns them again. (Experimental) </param>
        public static void ClearActiveSaveData(bool removeListeners = true, bool reloadActiveScenes = false)
        {
            if (activeSlot != -1)
            {
                int slot = activeSlot;
                DeleteSave(activeSlot);

                if (removeListeners || reloadActiveScenes)
                {
                    ClearSlot();

                    if (reloadActiveScenes)
                    {
                        int totalScenes = SceneManager.sceneCount;
                        List<int> loadedScenes = new List<int>();

                        Scene getActiveScene = SceneManager.GetActiveScene();
                        loadedScenes.Add(getActiveScene.buildIndex);

                        for (int i = 0; i < totalScenes; i++)
                        {
                            Scene getAdditionalScene = SceneManager.GetSceneAt(i);

                            if (getActiveScene != getAdditionalScene && getAdditionalScene.isLoaded)
                                loadedScenes.Add(getAdditionalScene.buildIndex);
                        }

                        int loadedSceneCount = loadedScenes.Count;

                        SceneManager.LoadScene(loadedScenes[0]);

                        if (loadedSceneCount > 1)
                        {
                            for (int i = 1; i < loadedSceneCount; i++)
                            {
                                SceneManager.LoadScene(loadedScenes[i], LoadSceneMode.Additive);
                            }
                        }
                    }

                }

                SetSlot(slot, false);
            }
        }

        /// <summary>
        /// Delete a save file based on a specific slot.
        /// </summary>
        /// <param name="slot"></param>
        public static void DeleteSave(int slot)
        {
            if (slot == activeSlot)
            {
                activeSlot = -1;
                activeSaveGame.Dispose();
                activeSaveGame = null;

                if (SaveSettings.Get().slotLoadBehaviour 
                    == SaveSettings.SlotLoadBehaviour.LoadTemporarySlot)
                {
                    SetSlotToTemporarySlot(true, false);
                }
            }

            MetaDataFileUtility.DeleteMetaData(GetSaveFileName(slot, ""));
            SaveFileUtility.DeleteSave(slot);
            PlayerPrefs.DeleteKey("SM-LastUsedSlot");
            OnDeletedSave.Invoke(slot);
        }

        /// <summary>
        /// Removes the active save file. Based on the save slot index.
        /// </summary>
        public static void DeleteSave()
        {
            DeleteSave(activeSlot);
        }

        /// <summary>
        /// Sends request to all saveables to store data to the active save game
        /// </summary>
        public static void SyncSave()
        {
            OnSyncSaveBegin.Invoke(activeSlot);

            if (activeSaveGame == null)
            {
                Debug.LogWarning("SaveMaster Request Save Failed: " +
                                 "No active SaveGame has been set. Be sure to call SetSaveGame(index)");
                return;
            }

            foreach (var saveable in saveables)
            {
                saveable.OnSaveRequest(activeSaveGame);
            }

            foreach (var item in saveInstanceManagers)
            {
                item.Value.OnSave(activeSaveGame);
            }

            OnSyncSaveDone.Invoke(activeSlot);
        }

        /// <summary>
        /// Sends request to all saveables to load data from the active save game
        /// </summary>
        public static void SyncLoad()
        {
            if (activeSaveGame == null)
            {
                Debug.LogWarning("SaveMaster Request Load Failed: " +
                                 "No active SaveGame has been set. Be sure to call SetSlot(index)");
                return;
            }

            foreach (var saveable in saveables)
            {
                saveable.OnLoadRequest(activeSaveGame);
            }

            foreach (var item in saveInstanceManagers)
            {
                item.Value.OnLoad(activeSaveGame);
            }
        }

        /// <summary>
        /// Resets the state of the saveables. As if they have never loaded or saved.
        /// </summary>
        public static void SyncReset()
        {
            if (activeSaveGame == null)
            {
                Debug.LogWarning("SaveMaster Request Load Failed: " +
                                 "No active SaveGame has been set. Be sure to call SetSlot(index)");
                return;
            }

            foreach (var saveable in saveables)
            {
                saveable.ResetState();
            }
        }

        /// <summary>
        /// Spawn a prefab that will be tracked & saved for a specific scene.
        /// </summary>
        /// <param name="source">Methodology to know where prefab came from </param>
        /// <param name="filePath">This is used to retrieve the prefab again from the designated source. </param>
        /// <param name="scene">Saved prefabs are bound to a specific scene. Easiest way to reference is by passing through (gameObject.scene).
        /// By default is uses the active scene. </param>
        /// <returns> Instance of saved prefab. </returns>
        public static GameObject SpawnSavedPrefab(InstanceSource source, string filePath, string customSource = "", Scene scene = default(Scene))
        {
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.Log("File path is empty");
                return null;
            }

            // If no scene has been specified, it will use the current active scene.
            if (scene == default(Scene))
            {
                scene = SceneManager.GetActiveScene();
            }

            if (duplicatedSceneHandles.Contains(scene.GetHashCode()))
            {
                Debug.Log(string.Format("Following scene has a duplicate name: {0}. " +
                    "Ensure to call SaveMaster.SpawnInstanceManager(scene, id) with a custom ID after spawning the scene.", scene.name));
                scene = SceneManager.GetActiveScene();
            }

            SaveInstanceManager saveIM;
            if (!saveInstanceManagers.TryGetValue(scene.GetHashCode(), out saveIM))
            {
                saveIM = SpawnInstanceManager(scene);
            }

            SavedInstance obj = saveIM.SpawnObject(source, filePath, customSourcePath: customSource);

            if (obj == null)
                return null;

            return obj.gameObject;
        }

        private static string GetSaveFileName(int slot, string fileName)
        {
            if ((slot == -1 && string.IsNullOrEmpty(fileName)) || !SaveMaster.IsSlotUsed(slot))
            {
                return activeSaveGame != null ? activeSaveGame.fileName : "";
            }

            if (slot != -1)
            {
                string slotFileName = SaveFileUtility.ObtainSlotFileName(slot);

                if (!string.IsNullOrEmpty(slotFileName))
                {
                    return slotFileName;
                }
                else
                {
                    return "";
                }
            }
            else if (!string.IsNullOrEmpty(fileName))
            {
                if (SaveFileUtility.IsSaveFileNameUsed(fileName))
                {
                    return fileName;
                }
                else
                {
                    return "";
                }
            }
            return "";
        }

        // Setting and getting metadata for savegames. This is useful if you don't want to actually load the save game
        // To obtain relevant information. Such as: Screenshots, time played, etc.

        public static bool GetMetaData(string id, Texture2D tex, int slot = -1, string fileName = "")
        {
            string saveFileName = GetSaveFileName(slot, fileName);

            if (string.IsNullOrEmpty(saveFileName))
            {
                return false;
            }

            using (var metaData = MetaDataFileUtility.GetMetaData(saveFileName))
                return metaData.GetData(id, tex);
        }

        public static bool GetMetaData(string id, out byte[] data, int slot = -1, string fileName = "")
        {
            string saveFileName = GetSaveFileName(slot, fileName);

            if (string.IsNullOrEmpty(saveFileName))
            {
                data = null;
                return false;
            }

            using (var metaData = MetaDataFileUtility.GetMetaData(saveFileName))
                return metaData.GetData(id, out data);
        }

        public static bool GetMetaData(string id, out string data, int slot = -1, string fileName = "")
        {
            string saveFileName = GetSaveFileName(slot, fileName);

            if (string.IsNullOrEmpty(saveFileName))
            {
                data = null;
                return false;
            }

            using (var metaData = MetaDataFileUtility.GetMetaData(saveFileName))
                return metaData.GetData(id, out data);
        }

        public static void SetMetaData(string id, string data, int slot = -1, string fileName = "")
        {
            string saveFileName = GetSaveFileName(slot, fileName);

            if (string.IsNullOrEmpty(saveFileName))
            {
                return;
            }

            using (var metaData = MetaDataFileUtility.GetMetaData(saveFileName))
                metaData.SetData(id, data);
        }

        public static void SetMetaData(string id, Texture2D data, int slot = -1, string fileName = "")
        {
            string saveFileName = GetSaveFileName(slot, fileName);

            if (string.IsNullOrEmpty(saveFileName))
            {
                return;
            }

            using (var metaData = MetaDataFileUtility.GetMetaData(saveFileName))
                metaData.SetData(id, data);
        }

        public static void SetMetaData(string id, byte[] data, int slot = -1, string fileName = "")
        {
            string saveFileName = GetSaveFileName(slot, fileName);

            if (string.IsNullOrEmpty(saveFileName))
            {
                return;
            }

            using (var metaData = MetaDataFileUtility.GetMetaData(saveFileName))
                metaData.SetData(id, data);
        }

        /// <summary>
        /// Helper method for obtaining specific Saveable data.
        /// </summary>
        /// <typeparam name="T"> Object type to retrieve </typeparam>
        /// <param name="classType">Object type to retrieve</param>
        /// <param name="slot"> Save slot to load data from </param>
        /// <param name="saveableId"> Identification of saveable </param>
        /// <param name="componentId"> Identification of saveable component </param>
        /// <param name="data"> Data that gets returned </param>
        /// <returns></returns>
        public static bool GetSaveableData<T>(int slot, string saveableId, string componentId, out T data)
        {
            if (IsSlotUsed(slot) == false)
            {
                data = default(T);
                return false;
            }

            SaveGame saveGame = SaveMaster.GetSave(slot, false);

            if (saveGame == null)
            {
                data = default(T);
                return false;
            }

            string dataString = saveGame.Get(string.Format("{0}-{1}", saveableId, componentId));

            if (!string.IsNullOrEmpty(dataString))
            {
                data = JsonUtility.FromJson<T>(dataString);

                if (data != null)
                    return true;
            }

            data = default(T);
            return false;
        }

        /// <summary>
        /// Helper method for obtaining specific Saveable data.
        /// </summary>
        /// <typeparam name="T"> Object type to retrieve </typeparam>
        /// <param name="classType">Object type to retrieve</param>
        /// <param name="saveableId"> Identification of saveable </param>
        /// <param name="componentId"> Identification of saveable component </param>
        /// <param name="data"> Data that gets returned </param>
        /// <returns></returns>
        public static bool GetSaveableData<T>(string saveableId, string componentId, out T data)
        {
            if (activeSlot == -1)
            {
                data = default(T);
                return false;
            }

            return GetSaveableData<T>(activeSlot, saveableId, componentId, out data);
        }

        /// <summary>
        /// Set a integer value in the current currently active save
        /// </summary>
        /// <param name="key"> Identifier to remember storage point </param>
        /// <param name="value"> Value to store </param>
        public static void SetInt(string key, int value)
        {
            if (HasActiveSaveLogAction("Set Int") == false) return;
            activeSaveGame.Set(string.Format("IVar-{0}", key), value.ToString(), "Global");
        }

        /// <summary>
        /// Get a integer value in the currently active save
        /// </summary>
        /// <param name="key"> Identifier to remember storage point </param>
        /// <param name="defaultValue"> In case it fails to obtain the value, return this value </param>
        /// <returns> Stored value </returns>
        public static int GetInt(string key, int defaultValue = -1)
        {
            if (HasActiveSaveLogAction("Get Int") == false) return defaultValue;
            var getData = activeSaveGame.Get(string.Format("IVar-{0}", key));
            return string.IsNullOrEmpty((getData)) ? defaultValue : int.Parse(getData);
        }

        /// <summary>
        /// Set a floating point value in the currently active save
        /// </summary>
        /// <param name="key"> Identifier for value </param>
        /// <param name="value"> Value to store </param>
        public static void SetFloat(string key, float value)
        {
            if (HasActiveSaveLogAction("Set Float") == false) return;
            activeSaveGame.Set(string.Format("FVar-{0}", key), value.ToString(), "Global");
        }

        /// <summary>
        /// Get a float value in the currently active save
        /// </summary>
        /// <param name="key"> Identifier to remember storage point </param>
        /// <param name="defaultValue"> In case it fails to obtain the value, return this value </param>
        /// <returns> Stored value </returns>
        public static float GetFloat(string key, float defaultValue = -1)
        {
            if (HasActiveSaveLogAction("Get Float") == false) return defaultValue;
            var getData = activeSaveGame.Get(string.Format("FVar-{0}", key));
            return string.IsNullOrEmpty((getData)) ? defaultValue : float.Parse(getData);
        }

        /// <summary>
        /// Set a bool value in the currently active save
        /// </summary>
        /// <param name="key"> Identifier to remember storage point</param>
        /// <param name="value"> Value to store </param>
        public static void SetBool(string key, bool value)
        {
            if (HasActiveSaveLogAction("Set Bool") == false) return;
            activeSaveGame.Set(string.Format("SVar-{0}", key), value.ToString(), "Global");
        }

        /// <summary>
        /// Get a bool value in the currently active save
        /// </summary>
        /// <param name="key">Identifier to remember storage point</param>
        /// <param name="defaultValue"> In case it fails to obtain the value, return this value </param>
        /// <returns></returns>
        public static bool GetBool(string key, bool defaultValue = false)
        {
            if (HasActiveSaveLogAction("Get Bool") == false) return defaultValue;
            var getData = activeSaveGame.Get(string.Format("FVar-{0}", key));
            return string.IsNullOrEmpty((getData)) ? defaultValue : bool.Parse(getData);
        }

        /// <summary>
        /// Set a string value in the currently active save
        /// </summary>
        /// <param name="key"> Identifier for value </param>
        /// <param name="value"> Value to store </param>
        public static void SetString(string key, string value)
        {
            if (HasActiveSaveLogAction("Set String") == false) return;
            activeSaveGame.Set(string.Format("SVar-{0}", key), value, "Global");
        }

        /// <summary>
        /// Get a string value in the currently active save
        /// </summary>
        /// <param name="key"> Identifier to remember storage point </param>
        /// <param name="defaultValue"> In case it fails to obtain the value, return this value </param>
        /// <returns> Stored value </returns>
        public static string GetString(string key, string defaultValue = "")
        {
            if (HasActiveSaveLogAction("Get String") == false) return defaultValue;
            var getData = activeSaveGame.Get(string.Format("SVar-{0}", key));
            return string.IsNullOrEmpty((getData)) ? defaultValue : getData;
        }

        private static bool HasActiveSaveLogAction(string action)
        {
            if (SaveMaster.GetActiveSlot() == -1)
            {
                Debug.LogWarning(string.Format("{0} Failed: no save slot set. Please call SetSaveSlot(int index)",
                    action));
                return false;
            }
            else return true;
        }

        /// <summary>
        /// Clean all currently saved prefabs. Useful when switching scenes.
        /// </summary>
        private static void ClearActiveSavedPrefabs()
        {
            int totalLoadedScenes = SceneManager.sceneCount;

            for (int i = 0; i < totalLoadedScenes; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                SaveInstanceManager saveIM;

                if (saveInstanceManagers.TryGetValue(scene.GetHashCode(), out saveIM))
                {
                    saveIM.DestroyAllObjects();
                }
            }
        }

        // Events

        /// <summary>
        /// Parameter 1 = newSlot, Parameter 2 = oldSlot
        /// Called when a slot change is initiated. Before any saveables are written to disk.
        /// </summary>
        public static Action<int, int> OnSlotChangeBegin
        {
            get { return instance.onSlotChangeBegin; }
            set { instance.onSlotChangeBegin = value; }
        }

        /// <summary>
        /// Parameter 1 = newSlot, Parameter 2 = oldSlot
        /// Called when a slot change is done. After all saveables have been written to disk and slot
        /// has changed.
        /// </summary>
        public static Action<int, int> OnSlotChangeDone
        {
            get { return instance.onSlotChangeDone; }
            set { instance.onSlotChangeDone = value; }
        }

        public static Action<int> OnSyncSaveBegin
        {
            get { return instance.onSyncSaveBegin; }
            set { instance.onSyncSaveBegin = value; }
        }

        public static Action<int> OnSyncSaveDone
        {
            get { return instance.onSyncSaveDone; }
            set { instance.onSyncSaveDone = value; }
        }

        public static Action<int> OnWritingToDiskBegin
        {
            get { return instance.onWritingToDiskBegin; }
            set { instance.onWritingToDiskBegin = value; }
        }

        public static Action<int> OnWritingToDiskDone
        {
            get { return instance.onWritingToDiskDone; }
            set { instance.onWritingToDiskDone = value; }
        }

        public static Action<int> OnDeletedSave
        {
            get { return instance.onDeletedSave; }
            set { instance.onDeletedSave = value; }
        }

        /// <summary>
        /// Sends notification if a save instance manager has spawned an instance
        /// </summary>
        public static Action<Scene, SavedInstance> OnSpawnedSavedInstance
        {
            get { return instance.onSpawnedSavedInstance; }
            set { instance.onSpawnedSavedInstance = value; }
        }

        private Action<int, int> onSlotChangeBegin = delegate { };
        private Action<int, int> onSlotChangeDone = delegate { };
        private Action<int> onSyncSaveBegin = delegate { };
        private Action<int> onSyncSaveDone = delegate { };
        private Action<int> onWritingToDiskBegin = delegate { };
        private Action<int> onWritingToDiskDone = delegate { };
        private Action<int> onDeletedSave = delegate { };
        private Action<Scene, SavedInstance> onSpawnedSavedInstance = delegate { };

        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogWarning("Duplicate save master found. " +
                                 "Ensure that the save master has not been added anywhere in your scene.");
                GameObject.Destroy(this.gameObject);
                return;
            }

            instance = this;

            var settings = SaveSettings.Get();

            switch (settings.slotLoadBehaviour)
            {
                case SaveSettings.SlotLoadBehaviour.LoadDefaultSlot:
                    SetSlot(settings.defaultSlot, true);
                    break;
                case SaveSettings.SlotLoadBehaviour.LoadTemporarySlot:
                    SetSlot(-2, true);
                    break;
                case SaveSettings.SlotLoadBehaviour.DontLoadSlot:
                    break;
                default:
                    break;
            }

            if (settings.trackTimePlayed)
            {
                StartCoroutine(IncrementTimePlayed());
            }

            if (settings.useHotkeys)
            {
                StartCoroutine(TrackHotkeyUsage());
            }

            if (settings.saveOnInterval)
            {
                StartCoroutine(AutoSaveGame());
            }
        }

        private IEnumerator AutoSaveGame()
        {
            WaitForSeconds wait = new WaitForSeconds(SaveSettings.Get().saveIntervalTime);

            while (true)
            {
                yield return wait;
                WriteActiveSaveToDisk();
            }
        }

        private IEnumerator TrackHotkeyUsage()
        {
            var settings = SaveSettings.Get();

            while (true)
            {
                yield return null;

                if (!settings.useHotkeys)
                {
                    continue;
                }

                if (Input.GetKeyDown(settings.wipeActiveSceneData))
                {
                    SaveMaster.WipeSceneData(SceneManager.GetActiveScene().name);
                }

                if (Input.GetKeyDown(settings.saveAndWriteToDiskKey))
                {
                    var stopWatch = new System.Diagnostics.Stopwatch();
                    stopWatch.Start();

                    WriteActiveSaveToDisk();

                    stopWatch.Stop();
                    Debug.Log(string.Format("Synced objects & Witten game to disk. MS: {0}", stopWatch.ElapsedMilliseconds.ToString()));
                }

                if (Input.GetKeyDown(settings.syncSaveGameKey))
                {
                    var stopWatch = new System.Diagnostics.Stopwatch();
                    stopWatch.Start();

                    SyncSave();

                    stopWatch.Stop();
                    Debug.Log(string.Format("Synced (Save) objects. MS: {0}", stopWatch.ElapsedMilliseconds.ToString()));
                }

                if (Input.GetKeyDown(settings.syncLoadGameKey))
                {
                    var stopWatch = new System.Diagnostics.Stopwatch();
                    stopWatch.Start();

                    SyncLoad();

                    stopWatch.Stop();
                    Debug.Log(string.Format("Synced (Load) objects. MS: {0}", stopWatch.ElapsedMilliseconds.ToString()));
                }
            }
        }

        private IEnumerator IncrementTimePlayed()
        {
            WaitForSeconds incrementSecond = new WaitForSeconds(1);

            while (true)
            {
                if (activeSlot != -1)
                {
                    activeSaveGame.timePlayed = activeSaveGame.timePlayed.Add(TimeSpan.FromSeconds(1));
                }

                yield return incrementSecond;
            }
        }

        // This will get called on android devices when they leave the game
        private void OnApplicationPause(bool pause)
        {
            if (!pause)
                return;

            if (!SaveSettings.Get().autoSaveOnExit)
                return;

            WriteActiveSaveToDisk();
        }

        private void OnApplicationQuit()
        {
            isQuittingGame = true;

            if (!SaveSettings.Get().autoSaveOnExit)
                return;

            // Saving to disk also happens when the last saveable is removed.
            // This ensures proper disposal & writing of the savegame
            // This will only get called if there were no active saveables to begin with
            if (saveables.Count == 0)
            {
                WriteActiveSaveToDisk();
                activeSaveGame.Dispose();
            }
            else
            {
                if (!invokedWritingToDiskEvent)
                {
                    // Waiting for all saveables to get destroyed means
                    // most listeners for this event would also be destroyed before it gets invoked.
                    OnWritingToDiskBegin.Invoke(activeSlot);
                    invokedWritingToDiskEvent = true;
                }
            }
        }
    }
}