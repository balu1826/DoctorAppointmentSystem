using DoctorAppointmentSystem.Model.Enums;

namespace DoctorAppointmentSystem.DTO
{
    public class PatientAppointmentItemDTO
    {
        public int AppointmentId { get; set; }

        public string? DoctorName { get; set; }

        public string? Specialization { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public decimal ConsultationFee { get; set; }

        public AppointmentStatus Status { get; set; }
    }
}
