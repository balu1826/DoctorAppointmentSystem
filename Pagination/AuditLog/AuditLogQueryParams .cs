namespace DoctorAppointmentSystem.Pagination.AuditLog
{
    public class AuditLogQueryParams : BaseQueryParams
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public string? Action { get; set; }
        public string? EntityType { get; set; }
        public string? UserId { get; set; }
    }
}
