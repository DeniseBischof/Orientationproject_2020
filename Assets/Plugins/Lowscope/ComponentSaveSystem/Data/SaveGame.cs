
using Lowscope.Saving.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lowscope.Saving.Data
{
    /// <summary>
    /// Container for all saved data.
    /// Placed into a slot (separate save file)
    /// </summary>
    [Serializable]
    public abstract class SaveGame : IDisposable
    {
        [NonSerialized] public TimeSpan timePlayed;
        [NonSerialized] public int gameVersion;
        [NonSerialized] public DateTime creationDate;
        [NonSerialized] public int timesSaved;
        [NonSerialized] public DateTime modificationDate;

        public string fileName { get; internal set; }

        public void SetFileName(string fileName)
        {
            this.fileName = fileName;
        }

        public abstract void ReadSaveFile(string savePath);
        public abstract void WriteSaveFile(SaveGame saveGame, string savePath);

        public abstract void OnBeforeWrite();
        public abstract void OnAfterLoad();
        public abstract void WipeSceneData(string sceneName);
        public abstract void Remove(string id);

        /// <summary>
        /// Assign any data to the given ID. If data is already present within the ID, then it will be overwritten.
        /// </summary>
        /// <param name="id"> Save Identification </param>
        /// <param name="data"> Data in a string format </param>
        public abstract void Set(string id, string data, string scene);

        /// <summary>
        /// Returns any data stored based on a identifier
        /// </summary>
        /// <param name="id"> Save Identification </param>
        /// <returns></returns>
        public abstract string Get(string id);

        protected static void Log(string text)
        {
#if UNITY_EDITOR
            if (SaveSettings.Get().showSaveFileUtilityLog)
            {
                Debug.Log(text);
            }
#endif
        }

        public abstract void Dispose();
    }
}