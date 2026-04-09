using DoctorAppointmentSystem.Model.Enums;

namespace DoctorAppointmentSystem.DTO
{
    public class DoctorAppointmentItemDTO
    {
        public int AppointmentId { get; set; }

        public required string PatientId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public AppointmentStatus Status { get; set; }
    }
}
