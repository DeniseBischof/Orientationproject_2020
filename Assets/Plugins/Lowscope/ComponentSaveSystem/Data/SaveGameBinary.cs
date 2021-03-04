using System.IO;
using System.Text;
using Lowscope.Saving.Enums;
using UnityEngine;

namespace Lowscope.Saving.Data
{
    // Just using a different save implementation. Functionally the same as JSON.
    public class SaveGameBinary : SaveGameJSON
    {
        public override void ReadSaveFile(string savePath)
        {
            using (FileStream stream = new FileStream(savePath, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(stream, Encoding.ASCII))
                {
                    reader.ReadString();
                    metaData.creationDate = reader.ReadString();
                    metaData.gameVersion = reader.ReadInt32();
                    metaData.timePlayed = reader.ReadString();
                    metaData.modificationDate = reader.ReadString();

                    int saveDataLength = reader.ReadInt32();

                    for (int i = 0; i < saveDataLength; i++)
                    {
                        saveData.Add(new Data()
                        {
                            data = reader.ReadString(),
                            guid = reader.ReadString(),
                            scene = reader.ReadString()
                        });
                    }
                }
            }
        }

        public override void WriteSaveFile(SaveGame saveGame, string savePath)
        {
            using (FileStream stream = new FileStream(savePath, FileMode.Create))
            {
                using (BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII))
                {
                    writer.Write("binary");
                    writer.Write(metaData.creationDate);
                    writer.Write(metaData.gameVersion);
                    writer.Write(metaData.timePlayed);
                    writer.Write(metaData.modificationDate);

                    int saveDataCount = saveData.Count;

                    writer.Write(saveDataCount); // Store storage count

                    for (int i = 0; i < saveDataCount; i++)
                    {
                        writer.Write(saveData[i].data);
                        writer.Write(saveData[i].guid);
                        writer.Write(saveData[i].scene);
                    }
                }
            }
        }
    }
}