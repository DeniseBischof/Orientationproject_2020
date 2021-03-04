using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lowscope.Saving.Data;
using Lowscope.Saving.Enums;
using Lowscope.Saving.Encryption;
using System;

#if UNITY_WEBGL
using System.Runtime.InteropServices;
#endif

namespace Lowscope.Saving.Core
{
    public class SaveFileUtility
    {
        // Required if you use no domain reloading.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetVariable()
        {
            encryption = null;
        }

        // Saving with WebGL requires a seperate DLL, which is included in the plugin.
#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void SyncFiles();

        [DllImport("__Internal")]
        private static extern void WindowAlert(string message);
#endif

        private static string fileExtentionName { get { return SaveSettings.Get().fileExtensionName; } }
        private static string gameFileName { get { return SaveSettings.Get().fileName; } }

        private static bool debugMode { get { return SaveSettings.Get().showSaveFileUtilityLog; } }

        private static AESEncryption encryption;

        private static string dataPath
        {
            get
            {
                return string.Format("{0}/{1}",
                    Application.persistentDataPath,
                    SaveSettings.Get().fileFolderName);
            }
        }

        private static void Log(string text)
        {
            if (debugMode)
            {
                Debug.Log(text);
            }
        }

        private static Dictionary<int, string> cachedSlotSaveFileNames;

        public static Dictionary<int, string> ObtainSlotSaveFileNames()
        {
            if (cachedSlotSaveFileNames != null)
            {
                return cachedSlotSaveFileNames;
            }

            Dictionary<int, string> slotSavePaths = new Dictionary<int, string>();

            // Create a directory if it doesn't exist yet
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            string[] filePaths = Directory.GetFiles(dataPath);

            string[] savePaths = filePaths.Where(path => path.EndsWith(fileExtentionName)).ToArray();

            int pathCount = savePaths.Length;

            for (int i = 0; i < pathCount; i++)
            {
                Log(string.Format("Found slot save file at: {0}", savePaths[i]));

                int getSlotNumber;

                string slotName = savePaths[i].Substring(dataPath.Length + gameFileName.Length + 1);

                if (int.TryParse(slotName.Substring(0, slotName.LastIndexOf(".")), out getSlotNumber))
                {
                    string name = string.Format("{0}{1}", gameFileName, slotName);
                    slotSavePaths.Add(getSlotNumber, name);
                }
            }

            cachedSlotSaveFileNames = slotSavePaths;

            return slotSavePaths;
        }

        public static void EncryptSaveFile(string savePath)
        {
            if (encryption == null)
            {
                encryption = new AESEncryption(SaveSettings.Get().encryptionKey, SaveSettings.Get().encryptionIV);
            }

            try
            {
                string data = encryption.Encrypt(Convert.ToBase64String(File.ReadAllBytes(savePath)));
                File.WriteAllBytes(savePath, Convert.FromBase64String(data));
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public static void DecryptSaveFile(string savePath)
        {
            if (encryption == null)
            {
                encryption = new AESEncryption(SaveSettings.Get().encryptionKey, SaveSettings.Get().encryptionIV);
            }

            try
            {
                string data = encryption.Decrypt(Convert.ToBase64String(File.ReadAllBytes(savePath)));
                File.WriteAllBytes(savePath, Convert.FromBase64String(data));
            }
            catch (Exception e)
            {
                Debug.Log(string.Format("Decrypt failed. File is potentially already unencryped. Error: {0}", e));
            }
        }

        private static bool GetFileStorageType(string savePath, out StorageType storageType)
        {
            byte[] bytes = new byte[17];
            using (System.IO.FileStream fs = new System.IO.FileStream(savePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                fs.Read(bytes, 0, 16);
            }
            string chkStr = System.Text.ASCIIEncoding.ASCII.GetString(bytes);

            if (chkStr.Contains("SQLite format"))
            {
                storageType = StorageType.SQLiteExperimental;
                return true;
            }
            else if (chkStr.Contains("binary"))
            {
                storageType = StorageType.Binary;
                return true;
            }
            else if (chkStr.Contains('{'))
            {
                storageType = StorageType.JSON;
                return true;
            }

            storageType = StorageType.Binary;

            return false;
        }

        internal static SaveGame CreateSaveGameInstance(StorageType storageType)
        {
            switch (storageType)
            {
                case StorageType.JSON: return new SaveGameJSON();
                case StorageType.Binary: return new SaveGameBinary();
                case StorageType.SQLiteExperimental: return new SaveGameSqlite();
                default:
                    break;
            }

            return null;
        }

        private static SaveGame LoadSave(string fileName, int slot = -1)
        {
            SaveGame getSave = null;

            string filePath = Path.Combine(dataPath, fileName);

            StorageType storageType = SaveSettings.Get().storageType;
            SaveFileValidation validation = SaveSettings.Get().fileValidation;
            EncryptionType encryptionType = SaveSettings.Get().encryptionType;

            if (encryptionType == EncryptionType.AES && storageType != StorageType.SQLiteExperimental)
            {
                DecryptSaveFile(filePath);
            }

            if (validation != SaveFileValidation.DontCheck)
            {
                StorageType getType;
                if (GetFileStorageType(filePath, out getType))
                {
                    if (getType != storageType)
                    {
                        switch (validation)
                        {
                            case SaveFileValidation.GiveError:
                                Debug.LogError(string.Format("Storage type of file: ({0}) " +
                                    "did not match file from settings: ({1}). Click to read more. \n" +
                                    "Set the File Validation mode in settings to convert it," +
                                    " or change the storage type to matching type.", getType, storageType));
                                return null;

                            case SaveFileValidation.ConvertToType:

                                if (getType != storageType)
                                {
                                    Debug.Log(string.Format("Converting save file from type: {0} to type: {1}", getType, storageType));

                                    getSave = CreateSaveGameInstance(getType);
                                    getSave.ReadSaveFile(filePath);
                                    getSave = (getSave as IConvertSaveGame).ConvertTo(storageType, filePath);

                                    if (getSave == null)
                                    {
                                        Debug.Log(string.Format("No converter available for type: {0} to type {1}", getType, storageType));
                                        return null;
                                    }
                                }

                                break;
                            case SaveFileValidation.Replace:

                                getSave = CreateSaveGameInstance(storageType);

                                if (slot != -1)
                                {
                                    WriteSave(getSave, slot);
                                }

                                return getSave;

                            default:
                                break;
                        }
                    }
                }
                else
                {
                    Debug.LogError("Failed to get storage type of save file. Potentially corrupted?\n" +
                        "Set validation to DontCheck if you want to try loading it anyway.");
                    return null;
                }
            }

            getSave = CreateSaveGameInstance(storageType);
            getSave.ReadSaveFile(filePath);
            getSave.SetFileName(Path.GetFileNameWithoutExtension(fileName));

            if (encryptionType == EncryptionType.AES && storageType != StorageType.SQLiteExperimental)
            {
                EncryptSaveFile(filePath);
            }

            if (getSave != null)
            {
                getSave.OnAfterLoad();
                return getSave;
            }
            else
            {
                Log(string.Format("Save file corrupted: {0}", filePath));
                return null;
            }
        }

        public static int[] GetUsedSlots()
        {
            int[] saves = new int[ObtainSlotSaveFileNames().Count];

            int counter = 0;

            foreach (int item in ObtainSlotSaveFileNames().Keys)
            {
                saves[counter] = item;
                counter++;
            }

            return saves;
        }

        public static int GetSaveSlotCount()
        {
            return ObtainSlotSaveFileNames().Count;
        }

        public static SaveGame LoadSave(int slot, bool createIfEmpty = false)
        {
            if (slot < 0 && slot != -2)
            {
                Debug.LogWarning("Attempted to load negative slot");
                return null;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
                SyncFiles();
#endif

            string savePath = "";

            if (ObtainSlotSaveFileNames().TryGetValue(slot, out savePath))
            {
                SaveGame saveGame = LoadSave(savePath, slot);

                if (saveGame == null)
                {
                    cachedSlotSaveFileNames.Remove(slot);
                    return null;
                }

                Log(string.Format("Succesful load at slot (from cache): {0}", slot));
                return saveGame;
            }
            else
            {
                if (!createIfEmpty)
                {
                    Log(string.Format("Could not load game at slot {0}", slot));
                }
                else
                {

                    Log(string.Format("Creating save at slot {0}", slot));

                    SaveGame saveGame = null;

                    switch (SaveSettings.Get().storageType)
                    {
                        case StorageType.JSON:
                            saveGame = new SaveGameJSON();
                            break;
                        case StorageType.Binary:
                            saveGame = new SaveGameBinary();
                            break;
                        case StorageType.SQLiteExperimental:
                            saveGame = new SaveGameSqlite();
                            break;
                        default:
                            break;
                    }

                    return saveGame;
                }

                return null;
            }
        }

        public static void WriteSave(SaveGame saveGame, int saveSlot = -1, string fileName = "")
        {
            string savePath = "";

            if (saveSlot != -1)
            {
                savePath = string.Format("{0}/{1}{2}{3}", dataPath, gameFileName, saveSlot.ToString(), fileExtentionName);
            }
            else
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    savePath = string.Format("{0}/{1}{2}", dataPath, fileName, fileExtentionName);
                }
                else
                {
                    Debug.LogError("Specified file name is empty");
                    return;
                }
            }

            // Check if we can save the file to the destination.
            var directoryName = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            if (!cachedSlotSaveFileNames.ContainsKey(saveSlot))
            {
                cachedSlotSaveFileNames.Add(saveSlot, savePath);
            }

            StorageType storageType = SaveSettings.Get().storageType;

            Log(string.Format("Saving game slot {0} to : {1}. Using storage type: {2}",
                saveSlot.ToString(), savePath, storageType));

            saveGame.SetFileName(Path.GetFileNameWithoutExtension(savePath));

            saveGame.OnBeforeWrite();

            saveGame.WriteSaveFile(saveGame, savePath);

#if UNITY_WEBGL && !UNITY_EDITOR
        SyncFiles();
#endif

            if (SaveSettings.Get().encryptionType == EncryptionType.AES && storageType != StorageType.SQLiteExperimental)
            {
                EncryptSaveFile(savePath);
            }
        }

        public static void DeleteSave(int slot)
        {
            string filePath = string.Format("{0}/{1}{2}{3}", dataPath, gameFileName, slot, fileExtentionName);

            if (File.Exists(filePath))
            {
                Log(string.Format("Succesfully removed file at {0}", filePath));
                File.Delete(filePath);

                if (cachedSlotSaveFileNames.ContainsKey(slot))
                {
                    cachedSlotSaveFileNames.Remove(slot);
                }
            }
            else
            {
                Log(string.Format("Failed to remove file at {0}", filePath));
            }

#if UNITY_WEBGL && !UNITY_EDITOR
        SyncFiles();
#endif
        }

        public static bool IsSlotUsed(int index)
        {
            return ObtainSlotSaveFileNames().ContainsKey(index);
        }

        public static bool IsSaveFileNameUsed(string fileName)
        {
            string filePath = string.Format("{0}/{1}{2}", dataPath, fileName, fileExtentionName);
            return File.Exists(filePath);
        }

        public static int GetAvailableSaveSlot()
        {
            int slotCount = SaveSettings.Get().maxSaveSlotCount;

            for (int i = 0; i < slotCount; i++)
            {
                if (!ObtainSlotSaveFileNames().ContainsKey(i))
                {
                    return i;
                }
            }

            return -1;
        }

        public static string ObtainSlotFileName(int slot)
        {
            string fileName = "";
            cachedSlotSaveFileNames.TryGetValue(slot, out fileName);

            if (!string.IsNullOrEmpty(fileName))
            {
                fileName = Path.GetFileNameWithoutExtension(fileName);
            }
            else
            {
                return string.Format("{0}{1}", gameFileName, slot);
            }

            return fileName;
        }
    }
}