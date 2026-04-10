namespace DoctorAppointmentSystem.DTO
{
    public class AdminAppointmentsResponseDTO
    {
        public List<AdminAppointmentDTO> PastAppointments { get; set; } = new();
        public List<AdminAppointmentDTO> UpcomingAppointments { get; set; } = new();
    }
}
