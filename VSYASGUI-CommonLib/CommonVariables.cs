namespace VSYASGUI_CommonLib
{
    /// <summary>
    /// Common variables which are shared between the sender and reciever.
    /// </summary>
    public static class CommonVariables
    {
        public const string RequestHeaderApiKeyName = "api-key";
        public const int MaxRequestSize = 4096; // If any massive requests get added, this needs to be changed.
    }
}
