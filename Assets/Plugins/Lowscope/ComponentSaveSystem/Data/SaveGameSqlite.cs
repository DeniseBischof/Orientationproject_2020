using Lowscope.Saving.Enums;
#if !UNITY_WEBGL
using SQLite4Unity3d;
#endif
using System;
using System.Collections.Generic;
using System.IO;

namespace Lowscope.Saving.Data
{

#if UNITY_WEBGL
    public class SaveGameSqlite : SaveGame
    {
#endif

#if !UNITY_WEBGL
    public class SaveGameSqlite : SaveGame, IConvertSaveGame
    {
        [Serializable]
        public class MetaData
        {
            [PrimaryKey, Unique]
            public int id { get; set; }
            public int gameVersion { get; set; }
            public string creationDate { get; set; }
            public string timePlayed { get; set; }
            public string modificationDate { get; set; }
        }

        [Serializable]
        public class Data
        {
            [PrimaryKey, Unique]
            public string id { get; set; }
            public string data { get; set; }
            public string scene { get; set; }
        }

        private bool initialized = false;
        private SQLiteConnection connection;

        private void Initialize(string savePath)
        {
            if (!initialized)
            {
                connection = new SQLiteConnection(savePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
                connection.CreateTable<Data>(CreateFlags.ImplicitIndex);
                connection.CreateTable<MetaData>();
                initialized = true;

                var metaData = connection.Find<MetaData>(0);

                if (metaData != null)
                {
                    gameVersion = metaData.gameVersion;
                    DateTime.TryParse(metaData.creationDate, out creationDate);
                    TimeSpan.TryParse(metaData.timePlayed, out timePlayed);
                    DateTime.TryParse(metaData.modificationDate, out modificationDate);
                }

                connection.BeginTransaction();
            }
        }

        public override void ReadSaveFile(string savePath)
        {
            Initialize(savePath);
        }

        public override void OnAfterLoad() { }
        public override void OnBeforeWrite() { modificationDate = DateTime.Now; }

        public override void Remove(string id)
        {
            connection.Delete<Data>(id);
        }

        public override void Set(string id, string data, string scene)
        {
            if (string.IsNullOrEmpty(id))
                return;

            connection.InsertOrReplace(new Data()
            {
                data = data,
                id = id,
                scene = scene
            });
        }

        public override string Get(string id)
        {
            var getData = connection.Find<Data>(id);

            if (getData == null)
            {
                return "";
            }
            else
            {
                return getData.data;
            }
        }

        public override void WipeSceneData(string sceneName)
        {
            foreach (var item in connection.Table<Data>().Where(x => x.scene == sceneName))
            {
                connection.Delete<Data>(item);
            }
        }

        public override void WriteSaveFile(SaveGame saveGame, string savePath)
        {
            if (initialized)
            {
                MetaData metaData = new MetaData()
                {
                    id = 0,
                    creationDate = (creationDate != default(DateTime) ? creationDate : DateTime.Now).ToString(),
                    gameVersion = gameVersion,
                    timePlayed = timePlayed.ToString(),
                    modificationDate = DateTime.Now.ToString()
                };

                connection.InsertOrReplace(metaData);
                connection.Commit();
                connection.BeginTransaction();
            }

            Initialize(savePath);
        }

        public override void Dispose()
        {
            connection.Close();
            connection.Dispose();
        }

        public SaveGame ConvertTo(StorageType storageType, string filePath)
        {
            if (storageType == StorageType.JSON || storageType == StorageType.Binary)
            {
                if (!initialized)
                    Initialize(filePath);

                var saveGame = storageType == StorageType.JSON ? new SaveGameJSON() : new SaveGameBinary();
                saveGame.gameVersion = this.gameVersion;
                saveGame.timePlayed = this.timePlayed;
                saveGame.creationDate = this.creationDate;
                saveGame.modificationDate = this.modificationDate;
                saveGame.fileName = this.fileName;
                saveGame.metaData = new SaveGameJSON.MetaData()
                {
                    creationDate = this.creationDate.ToString(),
                    gameVersion = this.gameVersion,
                    timePlayed = this.timePlayed.ToString(),
                    modificationDate = this.modificationDate.ToString()
                };

                saveGame.saveData = new List<SaveGameJSON.Data>();

                foreach (var item in connection.Table<Data>())
                {
                    saveGame.saveData.Add(new SaveGameJSON.Data()
                    {
                        data = item.data,
                        guid = item.id,
                        scene = item.scene
                    });
                }

                string tempPath = string.Format("{0}{1}", filePath, ".temp");
                string backupPath = string.Format("{0}{1}", filePath, ".old");

                saveGame.WriteSaveFile(saveGame, tempPath);
                Dispose();
                File.Replace(tempPath, filePath, backupPath);

                return saveGame;
            }
            else
            {
                return this;
            }
        }

#else
        // Empty, so code compiles.
        public override void Dispose() { }
        public override string Get(string id) { return ""; }
        public override void OnAfterLoad() { }
        public override void OnBeforeWrite() { }
        public override void ReadSaveFile(string savePath) { }
        public override void Remove(string id) { }
        public override void Set(string id, string data, string scene) { }
        public override void WipeSceneData(string sceneName) { }
        public override void WriteSaveFile(SaveGame saveGame, string savePath) { }
#endif
    }
}