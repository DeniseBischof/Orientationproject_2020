
#if UNITY_EDITOR
using UnityEditor;
#endif

using Lowscope.Saving.Components;
using UnityEngine;

namespace Lowscope.Saving
{
    /// <summary>
    /// Useful for users that need to call specific functionality through UnityEvents
    /// or custom tools. Just need to reference the scriptable objects and call the functionality.
    /// </summary>
    [CreateAssetMenu(fileName = "Save Master Bridge", menuName = "Saving/Save Master Bridge")]
    public class SaveMasterBridge : ScriptableObject
    {
        public void SetSlotTolastUsedSlot()
        {
            SaveMaster.SetSlotToLastUsedSlot(true);
        }

        public void SetToNewAvailableSlot()
        {
            int slot;
            SaveMaster.SetSlotToNewSlot(true, out slot);
        }

        public void ClearSlot()
        {
            SaveMaster.ClearSlot(false);
        }

        public void ClearSlotAndSaveables()
        {
            SaveMaster.ClearSlot();
        }

        public void DeleteActiveSave()
        {
            SaveMaster.DeleteSave();
        }

        public void DeleteSave(int slot)
        {
            SaveMaster.DeleteSave(slot);
        }

        public void WriteActiveSaveToDisk()
        {
            SaveMaster.WriteActiveSaveToDisk(true);
        }

        public void SetSlot(int slot)
        {
            SaveMaster.SetSlot(slot, true);
        }

        public void SetSlotWithoutSavingActive(int slot)
        {
            SaveMaster.SetSlotWithoutSave(slot);
        }

        public void SetToTemporarySlot(bool reloadSaveables)
        {
            SaveMaster.SetSlotToTemporarySlot(reloadSaveables);
        }

        public void WipeSceneData(string sceneName)
        {
            SaveMaster.WipeSceneData(sceneName, false);
        }

        public void WipeSceneDataAndActiveSaveables(string sceneName)
        {
            SaveMaster.WipeSceneData(sceneName, true);
        }

        public void WipeSaveable(Saveable saveable)
        {
            SaveMaster.WipeSaveable(saveable);
        }

        public void ClearListeners()
        {
            SaveMaster.ClearListeners(false);
        }

        public void ClearListenersAndSave()
        {
            SaveMaster.ClearListeners(true);
        }

        public void SaveListener(Saveable saveable)
        {
            SaveMaster.SaveListener(saveable);
        }

        public void LoadListener(Saveable saveable)
        {
            SaveMaster.LoadListener(saveable);
        }

        public void ReloadListener(Saveable saveable)
        {
            SaveMaster.ReloadListener(saveable);
        }

        public void RemoveListener(Saveable saveable)
        {
            SaveMaster.RemoveListener(saveable);
        }

        public void ClearActiveSaveData()
        {
            SaveMaster.ClearActiveSaveData(false, false);
        }

        public void ClearActiveSaveDataAndListeners()
        {
            SaveMaster.ClearActiveSaveData(true, false);
        }

        public void ClearActiveSaveDataAndListenersAndReloadScene()
        {
            SaveMaster.ClearActiveSaveData(true, true);
        }

        public void SyncSave()
        {
            SaveMaster.SyncSave();
        }

        public void SyncLoad()
        {
            SaveMaster.SyncLoad();
        }

        public void SyncReset()
        {
            SaveMaster.SyncReset();
        }

        public void SpawnSavedPrefabResources(string resourceName)
        {
            SaveMaster.SpawnSavedPrefab(InstanceSource.Resources, resourceName);
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(SaveMasterBridge))]
    [CanEditMultipleObjects]
    public class LookAtPointEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.HelpBox("This object can be used in events. It contains methods you can call. For instance on a button." +
                "You can remove this object if you do not need the functionality. Do note that functionality of this is more limited then C#.", MessageType.Info);
        }
    }

#endif

}