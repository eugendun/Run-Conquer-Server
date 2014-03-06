using System.Collections.Generic;

namespace Run_Conquer_Server.Models
{
    public class GameInstanceDto
    {
        public int Id { get; set; }
        public MapDto Map { get; set; }
        public ICollection<TeamDto> Teams { get; set; }
    }
}