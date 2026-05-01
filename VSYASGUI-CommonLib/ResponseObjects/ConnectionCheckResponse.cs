namespace VSYASGUI_CommonLib.ResponseObjects
{
    /// <summary>
    /// Response for a connection check. Keeps track of instance, otherwise no response is expected (only HTTP 200 OK).
    /// </summary>
    public class ConnectionCheckResponse : InstanceAwareResponseBase
    {
        public override bool ExpectsResponse => false;
    }
}
