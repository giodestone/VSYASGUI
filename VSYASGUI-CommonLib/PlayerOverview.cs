namespace VSYASGUI_CommonLib
{
    /// <summary>
    /// Represents some overview details. Designed as a storage class.
    /// </summary>
    /// <remarks>
    /// <c>class</c> to make it compatible with data grid.
    /// </remarks>
    public class PlayerOverview
    {
        public string Name { get; set; }
        public string ConnectionState { get; set; }
        public string PlayerUid { get; set; }
        public string Groups { get; set; }
        public string LastKnownName { get; set; }
        public string FirstJoinDate { get; set; }
        public string LastJoinDate { get; set; }
    }
}
