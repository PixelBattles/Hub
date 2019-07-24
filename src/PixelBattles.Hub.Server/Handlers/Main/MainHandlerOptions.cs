namespace PixelBattles.Hub.Server.Handlers.Main
{
    public class MainHandlerOptions
    {
        public int ChunkHandlerInactivityTimeout { get; set; }

        public int BattleHandlerInactivityTimeout { get; set; }

        public int CompactionTimeout { get; set; }
    }
}