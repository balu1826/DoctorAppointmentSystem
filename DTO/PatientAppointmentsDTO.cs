namespace DoctorAppointmentSystem.DTO
{
    public class PatientAppointmentsDTO
    {
        public List<PatientAppointmentItemDTO>? UpcomingAppointments { get; set; }

        public List<PatientAppointmentItemDTO>? CompletedAppointments { get; set; }
    }
}
