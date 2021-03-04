namespace Lowscope.Saving.Enums
{
    /// <summary>
    /// Defines all storage types used for Component Save System
    /// </summary>
    public enum SaveFileValidation
    {
        DontCheck = 0,
        GiveError = 1,
        ConvertToType = 2,
        Replace = 3
    }
}