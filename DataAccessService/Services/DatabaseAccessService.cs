using Common.DTO;
using DataAccessService.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DataAccessService.Services
{
    internal class DatabaseAccessService : IHostedService
    {
        private readonly ApplicationContext _context;
        public DatabaseAccessService(ApplicationContext context)
        {
            _context = context;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Patch(GameResultDTO gameResult)
        {
            var player = _context.Players
                .Where(x => x.Name == gameResult.PlayerName)
                .FirstOrDefault();
            var game = _context.Games
                .Where(x => x.Name == "SpiderSolitaire") //TODO: добавить название игры в PATCH запрос
                .FirstOrDefault();
            var stats = _context.PlayerStats
                .Where(x => x.Player == player && x.Game == game)
                .FirstOrDefault();
            stats.GamesPlayed++;
            if (gameResult.IsWin)
            {
                stats.GamesWon++;
            }
            _context.SaveChanges();
        }

        public PlayerStatsDTO Get(string playerName)
        {
            var player = _context.Players
                .Where(x => x.Name == playerName)
                .FirstOrDefault();
            var game = _context.Games
                .Where(x => x.Name == "SpiderSolitaire") //TODO: добавить название игры в GET запрос
                .FirstOrDefault();
            var stats = _context.PlayerStats
                .Where(x => x.Player == player && x.Game == game)
                .FirstOrDefault();
            var result = new PlayerStatsDTO
            {
                GamesPlayed = stats.GamesPlayed,
                GamesWon = stats.GamesWon,
            };
            return result;
        }
    }
}