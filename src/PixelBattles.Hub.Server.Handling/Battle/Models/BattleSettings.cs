using PixelBattles.Hub.Server.Handling.Chunk;
using System;

namespace PixelBattles.Hub.Server.Handling.Battle
{
    public class BattleSettings
    {
        public ChunkSettings ChunkSettings { get; set; }
        public int MaxHeightIndex { get; set; }
        public int MaxWidthIndex { get; set; }
        public int MinHeightIndex { get; set; }
        public int MinWidthIndex { get; set; }
        public int Cooldown { get; set; }
        public DateTime StartDateUTC { get; set; }
        public DateTime EndDateUTC { get; set; }
    }
}