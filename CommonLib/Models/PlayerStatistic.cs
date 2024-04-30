namespace Common.Models
{
    public class PlayerStatistic
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
    }
}
