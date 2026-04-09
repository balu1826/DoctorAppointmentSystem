using DoctorAppointmentSystem.Model.Enums;

namespace DoctorAppointmentSystem.DTO
{
    public class AdminAppointmentDTO
    {
        public int AppointmentId { get; set; }

        public required string DoctorName { get; set; }

    
        public required string PatientName { get; set; } 

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public required String  Status { get; set; }
    }
}
