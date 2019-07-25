namespace PixelBattles.Hub.Server.Handling.Main
{
    public class MainHandlerOptions
    {
        public int ChunkHandlerInactivityTimeout { get; set; }

        public int BattleHandlerInactivityTimeout { get; set; }

        public int CompactionTimeout { get; set; }
    }
}