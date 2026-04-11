namespace DoctorAppointmentSystem.Model
{
    public class SystemLog
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        public string? ApiEndpoint { get; set; }
      

        public string? HttpMethod { get; set; }

        public string? ExceptionMessage { get; set; }

        public string? StackTrace { get; set; }

        public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
