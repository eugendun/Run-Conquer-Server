using System.Collections.Generic;
namespace Run_Conquer_Server.Models
{
    public class TeamDto
    {
        public int Id { get; set; }
        public string Color { get; set; }
        public string Name { get; set; }
        public int GameInstanceId { get; set; }
        public GameInstanceDto GameInstance { get; set; }
        public ICollection<PlayerDto> Players { get; set; }
    }
}