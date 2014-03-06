namespace Run_Conquer_Server.Models
{
    public class PlayerDto
    {
        public int Id { get; set; }
        public int? TeamId { get; set; }
        public PositionType Position { get; set; }
        public TeamDto Team { get; set; }
    }
}