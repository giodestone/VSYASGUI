namespace VSYASGUI_CommonLib.RequestObjects
{
    /// <summary>
    /// Request certain lines from the command. Related: <seealso cref="VSYASGUI_CommonLib.ResponseObjects.ConsoleEntriesResponse"/>.
    /// </summary>
    public class ConsoleRequest : RequestBase
    {
        public override string Address => "/console";

        public long LineFrom { get; set; } = 0;
    }
}
