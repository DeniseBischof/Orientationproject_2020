using Lowscope.Saving.Data;
using System.IO;
using UnityEngine;

namespace Lowscope.Saving.Components
{
    [CreateAssetMenu(fileName = "Save Identifier Reference", menuName = "Saving/Save Identifier Reference")]
    public class SaveIdentifierVariable : ScriptableObject
    {
        [SerializeField] private string identifier = "";

        public string Identifier
        {
            get { return identifier; }
            set { this.identifier = value; }
        }

#if UNITY_EDITOR
        public static SaveIdentifierVariable CreateSaveIdentifierAsset(string identifier)
        {
            var asset = ScriptableObject.Instantiate(new SaveIdentifierVariable());
            asset.identifier = identifier;

            var settings = SaveSettings.Get();

            string saveFolder = settings.saveIdentifierReferenceFolder;
            string savePrefix = settings.saveIdentifierPrefix;

            string savePath = string.Format("Assets/{0}/{1}{2}.asset", saveFolder, savePrefix, identifier);

            string saveDirectory = string.Format("{0}/{1}", Application.dataPath, saveFolder);

            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }

            string uniquePath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(savePath);
            UnityEditor.AssetDatabase.CreateAsset(asset, uniquePath);

            return asset;
        }
#endif
    }
}