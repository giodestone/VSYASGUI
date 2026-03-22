namespace VSYASGUI_CommonLib.ResponseObjects
{
    /// <summary>
    /// Provides a list of the current overviews.
    /// 
    /// Response for <see cref="VSYASGUI_CommonLib.RequestObjects.PlayerOverviewRequest"/>.
    /// </summary>
    public class PlayerOverviewResponse : ResponseBase
    {
        public override bool ExpectsResponse => true;

        public string HashOfPlayerOverviews { get; set; }
        public List<PlayerOverview>? PlayerOverviews { get; set; }
    }
}
