namespace DoctorAppointmentSystem.Model
{
    public class AuditLog
    {
        public int Id { get; set; }

        public required string Action { get; set; } 

        public  required string PerformedByUserId { get; set; }

        public required string EntityType { get; set; } 

        public int? ReferenceId { get; set; } 

        public string? Description { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
