using PixelBattles.Chunkler.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Handlers
{
    public class ChunkHandler
    {
        private Guid _battleId;
        private ChunkKey _chunkKey;
        private IChunklerClient _chunklerClient;

        public ChunkHandler(Guid battleId, ChunkKey chunkKey, IChunklerClient chunklerClient)
        {
            _battleId = battleId;
            _chunkKey = chunkKey;
            _chunklerClient = chunklerClient;
        }

        public async Task<ChunkState> GetStateAsync()
        {
            var state = await _chunklerClient.GetChunkState(new Chunkler.ChunkKey
            {
                BattleId = _battleId,
                ChunkXIndex = _chunkKey.X,
                ChunkYIndex = _chunkKey.Y
            });

            return new ChunkState
            {
                ChangeIndex = state.ChangeIndex,
                Image = state.Image
            };
        }
    }
}
