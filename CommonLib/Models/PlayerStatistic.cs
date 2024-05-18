using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models
{
    public class PlayerStatistic
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public Game Game { get; set; }
        [Required]
        public Player Player { get; set; }
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
    }
}
