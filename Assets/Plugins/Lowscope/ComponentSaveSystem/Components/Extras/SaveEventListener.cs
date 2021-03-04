using UnityEngine;
using Lowscope.Saving;
using Lowscope.Saving.Core;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace Lowscope.Saving.Components
{
    [AddComponentMenu("Saving/Components/Extras/Save Event Listener"), DisallowMultipleComponent]
    public class SaveEventListener : MonoBehaviour
    {
        [System.Serializable]
        public class UnityEventSavedInstance : UnityEvent<Scene, SavedInstance> { }

        [System.Serializable]
        public class UnityEventSlotChange : UnityEvent<int, int> { }

        [System.Serializable]
        public class UnityEventInt : UnityEvent<int> { }

        [Header("Parameters (NewSlot, OldSlot)")]
        public UnityEventSlotChange EventOnSlotChangeBegin;

        public UnityEventSlotChange EventOnSlotChangeDone;

        [Header("Parameter (Slot)")]
        public UnityEventInt EventOnSyncSaveBegin;
        public UnityEventInt EventOnSyncSaveDone;
        public UnityEventInt EventOnWriteToDiskBegin;
        public UnityEventInt EventOnWriteToDiskDone;
        public UnityEventInt eventOnDeletedSave;

        [Header("Parameters (Scene, SavedInstance)")]
        public UnityEventSavedInstance EventOnSpawnedSavedInstance;

        private void Awake()
        {
            SaveMaster.OnDeletedSave += OnDeletedSave;
            SaveMaster.OnSlotChangeBegin += OnSlotChangeBegin;
            SaveMaster.OnSlotChangeDone += OnSlotChangeDone;
            SaveMaster.OnSyncSaveBegin += OnSaveSyncBegin;
            SaveMaster.OnSyncSaveDone += OnSaveSyncDone;
            SaveMaster.OnWritingToDiskBegin += OnWritingToDiskBegin;
            SaveMaster.OnWritingToDiskDone += OnWritingToDiskDone;
            SaveMaster.OnSpawnedSavedInstance += OnSpawnedSavedInstance;
        }

        private void OnDestroy()
        {
            SaveMaster.OnDeletedSave -= OnDeletedSave;
            SaveMaster.OnSlotChangeBegin -= OnSlotChangeBegin;
            SaveMaster.OnSlotChangeDone -= OnSlotChangeDone;
            SaveMaster.OnSyncSaveBegin -= OnSaveSyncBegin;
            SaveMaster.OnSyncSaveDone -= OnSaveSyncDone;
            SaveMaster.OnWritingToDiskBegin -= OnWritingToDiskBegin;
            SaveMaster.OnWritingToDiskDone -= OnWritingToDiskDone;
            SaveMaster.OnSpawnedSavedInstance -= OnSpawnedSavedInstance;
        }

        private void OnSpawnedSavedInstance(Scene arg1, SavedInstance arg2)
        {
            EventOnSpawnedSavedInstance.Invoke(arg1, arg2);
        }

        private void OnWritingToDiskDone(int obj)
        {
            EventOnWriteToDiskDone.Invoke(obj);
        }

        private void OnWritingToDiskBegin(int obj)
        {
            EventOnWriteToDiskBegin.Invoke(obj);
        }

        private void OnSaveSyncDone(int obj)
        {
            EventOnSyncSaveDone.Invoke(obj);
        }

        private void OnSaveSyncBegin(int obj)
        {
            EventOnSyncSaveBegin.Invoke(obj);
        }

        private void OnSlotChangeDone(int arg1, int arg2)
        {
            EventOnSlotChangeDone.Invoke(arg1, arg2);
        }

        private void OnSlotChangeBegin(int arg1, int arg2)
        {
            EventOnSlotChangeBegin.Invoke(arg1, arg2);
        }

        private void OnDeletedSave(int obj)
        {
            eventOnDeletedSave.Invoke(obj);
        }
    }
}