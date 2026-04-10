namespace DoctorAppointmentSystem.DTO
{
    public class AuditLogDTO
    {
        public int Id { get; set; }
        public  required string Action { get; set; }

        public  required string UserId { get; set; }

        public  required string EntityType { get; set; }

        public int? ReferenceId { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
