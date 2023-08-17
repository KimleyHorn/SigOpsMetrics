using System;

namespace SigOpsMetrics.API.Classes.DTOs
{
    /// <summary>
    /// Class to represent 1 row of TaskHistory table
    /// </summary>
    public class TaskInfoDTO
    {
        public Guid TaskId { get; set; }
        public DateTime DueDate { get; set; }
        public string Title { get; set; }
        public string TaskType { get; set; }
        public string TaskSubtype { get; set; }
        public string TaskSource { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTime? DateReported { get; set; }
        public DateTime? DateResolved { get; set; }
        public int? TimeToResolveInDays { get; set; }
        public int? TimeToResolveInHours { get; set; }
        public int? OverdueInDays { get; set; }
        public int? OverdueInHours { get; set; }
        public string Comment { get; set; }
        public string AssignedTo { get; set; }
        public string Subscribers { get; set; }
        public string MaintainedBy { get; set; }
        public string OwnedBy { get; set; }
        public Guid LocationId { get; set; }
        public string CustomIdentifier { get; set; }
        public string PrimaryRoute { get; set; }
        public string SecondaryRoute { get; set; }
        public string LocationComment { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Location { get; set; }
    }
}
