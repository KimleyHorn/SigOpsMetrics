namespace SigOpsMetrics.API.Classes.DTOs
{
    public class AverageDTO
    {
        public string label { get; set; }
        public double avg { get; set; }
        public double delta { get; set; }
        public string zoneGroup { get; set; }
        public double weight { get; set; }
    }
}
