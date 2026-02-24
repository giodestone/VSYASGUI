namespace VSYASGUI_CommonLib.ResponseObjects
{
    /// <summary>
    /// Signifies an error response.
    /// </summary>
    public sealed class ErrorResponse : ResponseBase
    {
        public override bool ExpectsResponse => true;
        public string error { get; set; } = "undefined";

    }
}
