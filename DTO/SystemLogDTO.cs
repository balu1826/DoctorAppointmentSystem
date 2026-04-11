namespace DoctorAppointmentSystem.DTO
{
    public class SystemLogDTO
    {
        public string? UserId { get; set; }

        public string? Endpoint { get; set; }

        public string? HttpMethod { get; set; }

        public string? ExceptionMessage { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
