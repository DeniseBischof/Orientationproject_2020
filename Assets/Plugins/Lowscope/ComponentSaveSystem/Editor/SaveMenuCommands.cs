using UnityEngine;
using UnityEditor;
using System.IO;
using Lowscope.Saving.Data;
using UnityEditor.SceneManagement;
using Lowscope.Saving.Components;
using Lowscope.Saving.Core;

namespace Lowscope.SaveMaster.EditorTools
{
    public class SaveMenuCommands
    {
        [UnityEditor.MenuItem(itemName: "Window/Saving/Open Save Location")]
        public static void OpenSaveLocation()
        {
            string dataPath = string.Format("{0}/{1}/", Application.persistentDataPath, SaveSettings.Get().fileFolderName);

#if UNITY_EDITOR_WIN
            dataPath = dataPath.Replace(@"/", @"\"); // Windows uses backward slashes
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            dataPath = dataPath.Replace("\\", "/"); // Linux and MacOS use forward slashes
#endif

            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            EditorUtility.RevealInFinder(dataPath);
        }

        [MenuItem("Window/Saving/Open Save Settings")]
        public static void OpenSaveSystemSettings()
        {
            Selection.activeInstanceID = SaveSettings.Get().GetInstanceID();
        }

        [MenuItem("Window/Saving/Utility/Save Identification/Wipe Save Identifications (Active Scene)")]
        public static void WipeSceneSaveIdentifications()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            int rootObjectCount = rootObjects.Length;

            // Get all Saveables, including children and inactive.
            for (int i = 0; i < rootObjectCount; i++)
            {
                foreach (Saveable item in rootObjects[i].GetComponentsInChildren<Saveable>(true))
                {
                    item.SaveIdentification = "";
                    item.OnValidate();
                }
            }
        }

        [MenuItem("Window/Saving/Utility/Save Identification/Wipe Save Identifications (Active Selection(s))")]
        public static void WipeActiveSaveIdentifications()
        {
            foreach (GameObject obj in Selection.gameObjects)
            {
                foreach (Saveable item in obj.GetComponentsInChildren<Saveable>(true))
                {
                    item.SaveIdentification = "";
                    item.OnValidate();
                }
            }
        }

        [MenuItem("Window/Saving/Utility/Encryption/Decrypt all save files")]
        public static void DecryptAllSaveFiles()
        {
            string saveFilePath = Path.Combine(Application.persistentDataPath, SaveSettings.Get().fileFolderName);

            foreach (var item in Directory.GetFiles(saveFilePath))
            {
                if (Path.GetExtension(item) == SaveSettings.Get().fileExtensionName)
                {
                    SaveFileUtility.DecryptSaveFile(item);
                }
            }
        }

        [MenuItem("Window/Saving/Utility/Encryption/Encrypt all save files")]
        public static void EncryptAllSaveFiles()
        {
            string saveFilePath = Path.Combine(Application.persistentDataPath, SaveSettings.Get().fileFolderName);

            foreach (var item in Directory.GetFiles(saveFilePath))
            {
                if (Path.GetExtension(item) == SaveSettings.Get().fileExtensionName)
                {
                    SaveFileUtility.EncryptSaveFile(item);
                }
            }
        }

        [MenuItem("Window/Saving/Utility/Encryption/Decrypt file")]
        public static void DecryptSaveFile()
        {
            string path = EditorUtility.OpenFilePanel("Select save game file", Application.persistentDataPath,
                SaveSettings.Get().fileExtensionName.TrimStart('.'));
            if (path.Length != 0)
            {
                SaveFileUtility.DecryptSaveFile(path);
            }

        }

        [MenuItem("Window/Saving/Utility/Encryption/Encrypt file")]
        public static void EncryptSaveFile()
        {
            string path = EditorUtility.OpenFilePanel("Select save game file", Application.persistentDataPath,
                SaveSettings.Get().fileExtensionName.TrimStart('.'));
            if (path.Length != 0)
            {
                SaveFileUtility.EncryptSaveFile(path);
            }

        }
    }
}