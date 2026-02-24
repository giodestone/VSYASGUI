namespace VSYASGUI_CommonLib.ResponseObjects
{
    /// <summary>
    /// For type system. Signifies that no response body is expected.
    /// </summary>
    public class NoResponse : ResponseBase
    {
        public override bool ExpectsResponse => false;
    }
}
