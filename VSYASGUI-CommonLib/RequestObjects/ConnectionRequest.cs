namespace VSYASGUI_CommonLib.RequestObjects
{
    /// <summary>
    /// For testing the connection. Does not expect a response class.
    /// </summary>
    public class ConnectionRequest : RequestBase
    {
        public override string Address => "/";
    }
}
