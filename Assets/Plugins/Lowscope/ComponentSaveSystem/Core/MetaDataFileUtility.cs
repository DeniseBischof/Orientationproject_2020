using UnityEngine;
using System.Collections;
using Lowscope.Saving.Data;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Lowscope.Saving.Core
{
    public class MetaDataFileUtility
    {
        private static string fileMetaDataExtentionName { get { return SaveSettings.Get().metaDataExtentionName; } }

        private static string dataPath
        {
            get
            {
                return string.Format("{0}/{1}",
                    Application.persistentDataPath,
                    SaveSettings.Get().fileFolderName);
            }
        }

        public class MetaData : IDisposable
        {
            private readonly string filePath;

            public MetaData(string filePath)
            {
                idData = new Dictionary<string, byte[]>();

                this.filePath = filePath;
                if (File.Exists(filePath))
                {
                    using (FileStream stream = new FileStream(filePath, FileMode.Open))
                    {
                        using (BinaryReader reader = new BinaryReader(stream, Encoding.ASCII))
                        {
                            int entries = reader.ReadInt32();
                            for (int i = 0; i < entries; i++)
                            {
                                string key = reader.ReadString();
                                int byteLength = reader.ReadInt32();
                                byte[] bytes = reader.ReadBytes(byteLength);
                                idData.Add(key, bytes);
                            }
                        }
                    }
                }
            }

            public void Dispose()
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII))
                    {
                        writer.Write(idData.Count);
                        foreach (var item in idData)
                        {
                            writer.Write(item.Key);
                            writer.Write(item.Value.Length);
                            writer.Write(item.Value);
                        }
                    }
                }
            }

            public Dictionary<string, byte[]> idData;

            public void SetData(string id, byte[] bytes)
            {
                if (bytes == null)
                    return;

                SetOrAddMetaData(id, bytes);
            }

            public void SetData(string id, Texture2D texture)
            {
                if (texture == null)
                    return;

                SetOrAddMetaData(id, ImageConversion.EncodeToPNG(texture));
            }

            public void SetData(string id, string data)
            {
                if (string.IsNullOrEmpty(data))
                    return;

                SetOrAddMetaData(id, Encoding.UTF8.GetBytes(data));
            }

            private void SetOrAddMetaData(string id, byte[] bytes)
            {
                if (idData.ContainsKey(id))
                {
                    idData[id] = bytes;
                }
                else
                {
                    idData.Add(id, bytes);
                }
            }

            public bool GetData(string id, out byte[] data)
            {
                byte[] getData;
                if (idData.TryGetValue(id, out getData))
                {
                    data = getData;
                    return true;
                }

                data = null;
                return false;
            }

            public bool GetData(string id, Texture2D tex)
            {
                byte[] getData;
                if (idData.TryGetValue(id, out getData))
                {
                    ImageConversion.LoadImage(tex, getData);
                    return true;
                }

                tex = null;
                return false;
            }

            public bool GetData(string id, out string data)
            {
                byte[] getData;
                if (idData.TryGetValue(id, out getData))
                {
                    data = System.Text.Encoding.UTF8.GetString(getData);
                    return true;
                }

                data = "";
                return false;
            }

            public void RemoveData(string id)
            {
                if (idData.ContainsKey(id))
                {
                    idData.Remove(id);
                }
            }
        }

        public static MetaData[] GetAllMetaData()
        {
            string[] filePaths = Directory.GetFiles(dataPath);

            string[] savePaths = filePaths.Where(path => path.EndsWith(fileMetaDataExtentionName)).ToArray();

            int pathCount = savePaths.Length;
            MetaData[] metaDataArray = new MetaData[pathCount];

            for (int i = 0; i < pathCount; i++)
            {
                metaDataArray[i] = new MetaData(savePaths[i]);
            }

            return metaDataArray;
        }

        public static MetaData GetMetaData(string fileName)
        {
            return new MetaData(string.Format("{0}{1}", Path.Combine(dataPath, fileName), fileMetaDataExtentionName));
        }

        internal static void DeleteMetaData(string fileName)
        {
            string filePath = string.Format("{0}{1}", Path.Combine(dataPath, fileName), fileMetaDataExtentionName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}