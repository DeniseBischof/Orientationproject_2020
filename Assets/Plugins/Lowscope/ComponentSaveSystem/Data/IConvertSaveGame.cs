using Lowscope.Saving.Data;
using Lowscope.Saving.Enums;

namespace Lowscope.Saving.Data
{
    public interface IConvertSaveGame
    {
        SaveGame ConvertTo(StorageType storageType, string filePath);
    }
}
