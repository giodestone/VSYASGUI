namespace VSYASGUI_CommonLib.ResponseObjects
{
    /// <summary>
    /// Relevant response for <seealso cref="VSYASGUI_CommonLib.RequestObjects.ConnectionRequest"/>.
    /// </summary>
    public class ConnectionCheckResponse : InstanceAwareResponseBase
    {
        public override bool ExpectsResponse => false;
    }
}
