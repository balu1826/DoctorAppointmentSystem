namespace DoctorAppointmentSystem.DTO
{
    public class DoctorAppointmentsDTO
    {
        public List<DoctorAppointmentItemDTO>? UpcomingAppointments { get; set; }

        public List<DoctorAppointmentItemDTO>? CompletedAppointments { get; set; }
    }
}
