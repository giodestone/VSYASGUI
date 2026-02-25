namespace VSYASGUI_WFP_App.MVVM.Models
{
    /// <summary>
    /// Provides a way of representing if an error has occurred.
    /// </summary>
    internal enum Error
    {
        Ok = 0,
        General,
        Connection,
        Unauthorised,
        Cancelled,
        Deserialisation,
        NotSent


    }
}
