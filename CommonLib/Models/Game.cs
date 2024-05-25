using System.ComponentModel.DataAnnotations;

namespace Common.Models
{
    public class Game
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
    }
}
