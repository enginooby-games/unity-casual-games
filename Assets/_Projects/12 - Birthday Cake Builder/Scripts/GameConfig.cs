namespace Devdy.BirthdayCake
{
    /// <summary>
    /// Holds all runtime configuration values for the game.
    /// Values are refreshed from SROptions when game starts/restarts.
    /// </summary>
    public class GameConfig
    {
        public int TotalLayers { get; private set; }
        public float DropSpeed { get; private set; }
        public float StabilityThreshold { get; private set; }
        public int CandleCount => 3;
        public float SizeReduction { get; private set; }

        public GameConfig()
        {
            RefreshFromSROptions();
        }

        /// <summary>
        /// Updates config values from SROptions runtime parameters.
        /// </summary>
        public void RefreshFromSROptions()
        {
            TotalLayers = SROptions.Current.BirthdayCake_TotalLayers;
            DropSpeed = SROptions.Current.BirthdayCake_DropSpeed;
            StabilityThreshold = SROptions.Current.BirthdayCake_StabilityThreshold;
            SizeReduction = SROptions.Current.BirthdayCake_SizeReduction;
        }
    }
}