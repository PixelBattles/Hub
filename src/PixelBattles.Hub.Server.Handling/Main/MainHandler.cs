using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nito.AsyncEx;
using PixelBattles.Hub.Server.Handlers.Battle;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PixelBattles.Hub.Server.Handling.Main
{
    internal class MainHandler : IMainHandler
    {
        private readonly IBattleHandlerFactory _battleHandlerFactory;
        private readonly ConcurrentDictionary<long, AsyncLazy<BattleHandler>> _battleHandlers;
        private readonly Dictionary<long, BattleHandler> _compactedBattleHandlers;
        private readonly MainHandlerOptions _mainHandlerOptions;
        private readonly ILogger _logger;

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _compactionTask;

        public MainHandler(
            IBattleHandlerFactory battleHandlerFactory,
            IOptions<MainHandlerOptions> mainHandlerOptions,
            ILogger<MainHandler> logger)
        {
            _battleHandlerFactory = battleHandlerFactory ?? throw new ArgumentNullException(nameof(battleHandlerFactory));
            _mainHandlerOptions = mainHandlerOptions.Value ?? throw new ArgumentNullException(nameof(mainHandlerOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _battleHandlers = new ConcurrentDictionary<long, AsyncLazy<BattleHandler>>();
            _compactedBattleHandlers = new Dictionary<long, BattleHandler>();

            _cancellationTokenSource = new CancellationTokenSource();
            _compactionTask = Task.Factory.StartNew(RunCompactionAsync, TaskCreationOptions.LongRunning);
        }

        public async Task<IBattleHandlerSubscription> GetBattleHandlerAndSubscribeAsync(long battleId)
        {
            while (true)
            {
                var battleHandlerLazy = _battleHandlers.GetOrAdd(
                    key: battleId,
                    valueFactory: key => new AsyncLazy<BattleHandler>(
                        factory: () => _battleHandlerFactory.CreateBattleHandlerAsync(battleId),
                        flags: AsyncLazyFlags.RetryOnFailure));

                var battleHandler = await battleHandlerLazy;

                if (battleHandler == null)
                {
                    throw new InvalidOperationException($"Battle {battleId} is not found.");
                }

                battleHandler.IncrementSubscriptionCounter();
                if (battleHandler.IsClosing)
                {
                    battleHandler.DecrementSubscriptionCounter();
                    continue;
                }
                var subscription = new BattleHandlerSubscription(battleHandler);
                return subscription;
            }
        }

        private async Task<(int battleHandlersNotCompacted, int battleHandlersCompacted)> CompactBattleHandlersAsync()
        {
            int battleHandlersNotCompacted = 0;
            int battleHandlersCompacted = 0;
            var unusedBattleHandlerTicksUTCLimit = DateTime.UtcNow.Ticks - _mainHandlerOptions.BattleHandlerInactivityTimeout;
            foreach (var battleHandlerLazy in _battleHandlers.Values)
            {
                var battleHandler = await battleHandlerLazy;
                var (activeChunksLeft, compactedChunksLeft) = await CompactBattleHandlerAsync(unusedBattleHandlerTicksUTCLimit, battleHandler);

                if (activeChunksLeft == 0
                    && compactedChunksLeft == 0
                    && battleHandler.SubscriptionCounter == 0
                    && battleHandler.LastUpdatedTicksUTC < unusedBattleHandlerTicksUTCLimit
                    && !_compactedBattleHandlers.ContainsKey(battleHandler.BattleId))
                {
                    if (_battleHandlers.TryRemove(battleHandler.BattleId, out var ignore))
                    {
                        battleHandler.MarkAsClosing();
                        _compactedBattleHandlers.Add(battleHandler.BattleId, battleHandler);
                        ++battleHandlersCompacted;
                    }
                    else
                    {
                        throw new InvalidOperationException("BattleHandler was removed during compaction in unexpected way.");
                    }
                }
                else
                {
                    ++battleHandlersNotCompacted;
                }
            }
            return (battleHandlersNotCompacted, battleHandlersCompacted);
        }

        private async Task<(int battleHandlersNotRemoved, int battleHandlersRemoved)> ClearCompactedBattleHandlersAsync()
        {
            int battleHandlersNotRemoved = 0;
            int battleHandlersRemoved = 0;
            if (_compactedBattleHandlers.Count == 0)
            {
                return (battleHandlersNotRemoved, battleHandlersRemoved);
            }

            var unusedBattleHandlerTicksUTCLimit = DateTime.UtcNow.Ticks - _mainHandlerOptions.BattleHandlerInactivityTimeout;
            var battleHandlersToDelete = new List<IBattleHandler>(_compactedBattleHandlers.Count);

            foreach (var battleHandler in _compactedBattleHandlers.Values)
            {
                var (activeChunksLeft, compactedChunksLeft) = await CompactBattleHandlerAsync(unusedBattleHandlerTicksUTCLimit, battleHandler);

                if (activeChunksLeft == 0
                    && compactedChunksLeft == 0
                    && battleHandler.SubscriptionCounter == 0
                    && battleHandler.LastUpdatedTicksUTC < unusedBattleHandlerTicksUTCLimit)
                {
                    battleHandlersToDelete.Add(battleHandler);
                    ++battleHandlersRemoved;
                }
                else
                {
                    ++battleHandlersNotRemoved;
                }
            }

            foreach (var battleHandler in battleHandlersToDelete)
            {
                if (!_compactedBattleHandlers.Remove(battleHandler.BattleId))
                {
                    throw new InvalidOperationException("BattleHandler was deleted in unexpected way from compaction.");
                }
            }
            return (battleHandlersNotRemoved, battleHandlersRemoved);
        }

        private async Task<(int activeChunkHandlersLeft, int compactedChunkHandlersLeft)> CompactBattleHandlerAsync(long chunkIncativityUTCLimit, BattleHandler battleHandler)
        {
            var (chunkHandlersNotRemoved, chunkHandlersRemoved) = await battleHandler.ClearCompactedChunkHandlersAsync();
            var (chunkHandlersNotCompacted, chunkHandlersCompacted) = await battleHandler.CompactChunkHandlersAsync(chunkIncativityUTCLimit);
            _logger.LogInformation($"Battle {battleHandler.BattleId} compaction finished. ChunkHandlers: Active={chunkHandlersNotCompacted}, Compacted={chunkHandlersCompacted + chunkHandlersNotRemoved}, Removed={chunkHandlersRemoved}");
            return (chunkHandlersNotCompacted, chunkHandlersCompacted + chunkHandlersNotRemoved);
        }

        private async Task RunCompactionAsync()
        {
            var token = _cancellationTokenSource.Token;
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(_mainHandlerOptions.CompactionTimeout, token);
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                try
                {
                    var (battleHandlersNotRemoved, battleHandlersRemoved) = await ClearCompactedBattleHandlersAsync();
                    var (battleHandlersNotCompacted, battleHandlersCompacted) = await CompactBattleHandlersAsync();
                    stopWatch.Stop();
                    _logger.LogInformation($"{nameof(MainHandler)} compaction finished within {stopWatch.ElapsedMilliseconds} ms. BattleHandlers: Active={battleHandlersNotCompacted}, Compacted={battleHandlersCompacted + battleHandlersNotRemoved}, Removed={battleHandlersRemoved}");
                }
                catch (Exception exception)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    stopWatch.Stop();
                    _logger.LogError(exception, "Error while running compaction.");
                }
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            try
            {
                _compactionTask.Wait();
            }
            catch (Exception)
            {
                //ignore
            }
            
            _compactionTask.Dispose();
            _cancellationTokenSource.Dispose();

            foreach (var battleHandler in _battleHandlers.Values)
            {
                battleHandler.Task.Result.Dispose();
            }

            foreach (var battleHandler in _compactedBattleHandlers.Values)
            {
                battleHandler.Dispose();
            }
        }
    }
}