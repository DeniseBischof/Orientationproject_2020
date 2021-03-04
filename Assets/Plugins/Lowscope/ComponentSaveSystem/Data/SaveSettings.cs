using Lowscope.Saving.Encryption;
using Lowscope.Saving.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Lowscope.Saving.Data
{
    public class SaveSettings : ScriptableObject
    {
        private static SaveSettings instance;

        private void OnDestroy()
        {
            instance = null;
        }

        public static SaveSettings Get()
        {
            if (instance != null)
            {
                return instance;
            }

            var savePluginSettings = Resources.Load("Save Plugin Settings", typeof(SaveSettings)) as SaveSettings;

#if UNITY_EDITOR
            // In case the settings are not found, we create one
            if (savePluginSettings == null)
            {
                return CreateFile();
            }
#endif

            // In case it still doesn't exist, somehow it got removed.
            // We send a default instance of SavePluginSettings.
            if (savePluginSettings == null)
            {
                Debug.LogWarning("Could not find SavePluginsSettings in resource folder, did you remove it? Using default settings.");
                savePluginSettings = ScriptableObject.CreateInstance<SaveSettings>();
            }

            instance = savePluginSettings;

            return instance;
        }

#if UNITY_EDITOR

        public static SaveSettings CreateFile()
        {
            string resourceFolderPath = string.Format("{0}/{1}", Application.dataPath, "Resources");
            string filePath = string.Format("{0}/{1}", resourceFolderPath, "Save Plugin Settings.asset");

            // In case the directory doesn't exist, we create a new one.
            if (!Directory.Exists(resourceFolderPath))
            {
                UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
            }

            // Check if the settings file exists in the resources path
            // If not, we create a new one.
            if (!File.Exists(filePath))
            {
                instance = ScriptableObject.CreateInstance<SaveSettings>();
                instance.legacyDynamicComponentNames = false; // In case of a new user, dont use legacy
                UnityEditor.AssetDatabase.CreateAsset(instance, "Assets/Resources/Save Plugin Settings.asset");
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();

                return instance;
            }
            else
            {
                return Resources.Load("Save Plugin Settings", typeof(SaveSettings)) as SaveSettings;
            }
        }

        private void OnValidate()
        {
            this.fileExtensionName = ValidateString(fileExtensionName, ".savegame", false);
            this.fileFolderName = ValidateString(fileFolderName, "SaveData", true);
            this.fileName = ValidateString(fileName, "Slot", true);

            if (fileExtensionName[0] != '.')
            {
                Debug.LogWarning("SaveSettings: File extension name needs to start with a .");
                fileExtensionName = string.Format(".{0}", fileExtensionName);
            }

            if (assetVersion != assetVersionTarget)
            {
                assetVersion = assetVersionTarget;
                slotLoadBehaviour = loadDefaultSlotOnStart ? SlotLoadBehaviour.LoadDefaultSlot
                    : SlotLoadBehaviour.DontLoadSlot;
            }
            else
            {
                // Compatibility in case you go back a version.
                switch (slotLoadBehaviour)
                {
                    case SlotLoadBehaviour.LoadDefaultSlot:
                        loadDefaultSlotOnStart = true;
                        break;
                    case SlotLoadBehaviour.LoadTemporarySlot:
                        break;
                    case SlotLoadBehaviour.DontLoadSlot:
                        loadDefaultSlotOnStart = false;
                        break;
                    default:
                        break;
                }
            }
        }

        private string ValidateString(string input, string defaultString, bool allowWhiteSpace)
        {
            if (string.IsNullOrEmpty(input) || (!allowWhiteSpace && input.Any(Char.IsWhiteSpace)))
            {
                Debug.LogWarning(string.Format("SaveSettings: Set {0} back to {1} " +
                                               "since it was empty or has whitespace.", input, defaultString));
                return defaultString;
            }
            else
            {
                return input;
            }
        }

#endif

        // Used to depricate specific fields and update others using OnValidate.
#if UNITY_EDITOR
        [HideInInspector, SerializeField] private int assetVersion;
        [HideInInspector, SerializeField] private int assetVersionTarget = 1;
        [HideInInspector, SerializeField] private bool loadDefaultSlotOnStart = true;
#endif

        public enum SlotLoadBehaviour
        {
            LoadDefaultSlot,
            LoadTemporarySlot,
            DontLoadSlot
        };

        [Header("Initialization")]
        [Tooltip("This slot will never be saved by default. Can be used to have some kind of saved state during play." +
            "You can save the data to a slot by using the SaveMaster.SetSlot(slotNumber ,keepActiveSaveData : true)")]
        public SlotLoadBehaviour slotLoadBehaviour = SlotLoadBehaviour.LoadDefaultSlot;

        [Range(0, 299)]
        public int defaultSlot = 0;

        [Header("Storage Settings - Save Files")]
        [Tooltip("Defines in what way to write savedata.Writing to JSON is most readable, " +
            "Binary is a bit faster in terms of write/read. SQlite is good for big projects with thousands of objects that need to be saved.")]
        public StorageType storageType = StorageType.JSON;
        [Tooltip("Apply a check or conversion if a storage type is different." +
            "\n\n-- Dontcheck --\nTry to read the file as given storage type no matter what" +
            "\n\n-- Give Error --\nSends out a Unity Editor Error if type does not match. Returns a null save file" +
            "\n\n-- Convert To Type --\nWill read out file with original type, and convert it to new storage type" +
            "\n\n-- Replace --\nIf the type is different, replace it with a new file. WARNING: Replace will remove all data!")]
        public SaveFileValidation fileValidation = SaveFileValidation.GiveError;
        public string fileExtensionName = ".savegame";
        public string fileFolderName = "SaveData";
        public string fileName = "Slot";
        public string metaDataExtentionName = ".metadata";

        [Header("Storage Settings - Save Identification")]
        public string saveIdentifierReferenceFolder = "Saving";
        public string saveIdentifierPrefix = "SaveId-";

        [Header("Storage Settings - Encryption")]
        [Tooltip("This is mainly to prevent simple users to access save files and change the contents." +
            "Keep in mind that any experienced user is able to decompile the game and obtain the key somehow." +
            "Do note, this functionality is not supported for SQLite")]
        public EncryptionType encryptionType;
        public string encryptionKey;
        public string encryptionIV;

        [Header("Storage Settings - JSON")]
        public bool useJsonPrettyPrint = true;
        [Tooltip("The old methodology added some garbage before the actual json text." +
            "This was because it was written as a BOM. Enabling this will make the save file unreadable for" +
            "an older version of the Component Save System then 1.1. This is currently left off for this reason.")]
        public bool legacyJSONWriting = false;

        [Header("Configuration")]
        [Range(1, 300)]
        public int maxSaveSlotCount = 300;
        [Tooltip("The save system will increment the time played since load")]
        public bool trackTimePlayed = true;
        [Tooltip("When you disable this, writing the game only happens when you call SaveMaster.Save()")]
        public bool autoSaveOnExit = true;
        [Tooltip("Should the game get saved when switching between game saves?")]
        public bool autoSaveOnSlotSwitch = true;

        [Header("Auto Save")]
        [Tooltip("Automatically save to the active slot based on a time interval, useful for WEBGL games")]
        public bool saveOnInterval = false;
        [Tooltip("Time interval in seconds before the autosave happens"), Range(1, 3000)]
        public int saveIntervalTime = 1;

        [Header("Saveable")]
        [Tooltip("Will do a check if object has already been instantiated with the ID")]
        public bool resetSaveableIdOnDuplicate = true;
        [Tooltip("Will do a check if object is serialized under a different scene name")]
        public bool resetSaveableIdOnNewScene = false;
        [Tooltip("Default generated guid length for a game object")]
        [Range(5, 36)]
        public int gameObjectGuidLength = 5;
        [Tooltip("Default generated guid length for a component")]
        [Range(5, 36)]
        public int componentGuidLength = 5;
        [Tooltip("Script change: Dynamic components used to have components with <gameObjectName>-gameObject-01 etc." +
            "This has been replaced with proper naming. <gameObjetName>-<ScriptName>-01")]
        public bool legacyDynamicComponentNames = true;

        [Header("Saveable Prefabs")]
        [Tooltip("Automatically remove saved instances when changing slots")]
        public bool cleanSavedPrefabsOnSlotSwitch = true;
        [Tooltip("Cleans up any saved prefab instances that have no saved data tied to them.")]
        public bool cleanEmptySavedPrefabs = true;

        [Header("Extras")]
        public bool useHotkeys = false;
        public KeyCode saveAndWriteToDiskKey = KeyCode.F2;
        public KeyCode syncSaveGameKey = KeyCode.F4;
        public KeyCode syncLoadGameKey = KeyCode.F5;
        public KeyCode wipeActiveSceneData = KeyCode.F6;

        [Header("Debug (Unity Editor Only)")]
        public bool showSaveFileUtilityLog = false;
    }
}