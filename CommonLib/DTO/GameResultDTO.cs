using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class GameResultDTO
    {
        public string? GameName { get; set; }
        public string? PlayerName { get; set; }
        public bool IsWin { get; set; } = false;

    }
}
