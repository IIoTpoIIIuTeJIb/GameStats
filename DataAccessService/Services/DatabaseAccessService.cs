using Common.DTO;
using Common.Models;
using DataAccessService.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DataAccessService.Services
{
    internal class DatabaseAccessService
    {
        private readonly ApplicationContext _context;
        public DatabaseAccessService(ApplicationContext context)
        {
            _context = context;
        }

        public void Patch(GameResultDTO gameResult)
        {
            var player = _context.Players
                .Where(x => x.Name == gameResult.PlayerName)
                .FirstOrDefault();
            var game = _context.Games
                .Where(x => x.Name == gameResult.GameName)
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
            if (player is null)
            {
                player = new Common.Models.Player()
                { Name = playerName };
                _context.Players.Add(player);
                _context.SaveChanges();
            }
            var game = _context.Games
                .Where(x => x.Name == "SpiderSolitaire") //TODO: добавить название игры в GET запрос
                .FirstOrDefault();
            if (game is null)
            {
                game = new Common.Models.Game()
                {
                    Name = "SpiderSolitaire"
                };
                _context.Games.Add(game);
                _context.SaveChanges();
            }
            var stats = _context.PlayerStats
                .Where(x => x.Player == player && x.Game == game)
                .FirstOrDefault();
            if (stats is null)
            {
                stats = new Common.Models.PlayerStatistic()
                {
                    Game = game,
                    Player = player
                };
                _context.PlayerStats.Add(stats);
                _context.SaveChanges();
            };
            var result = new PlayerStatsDTO
            {
                GamesPlayed = stats.GamesPlayed,
                GamesWon = stats.GamesWon,
            };
            return result;
        }
    }
}