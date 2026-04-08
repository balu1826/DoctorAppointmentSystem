using DoctorAppointmentSystem.Model.Enums; 
namespace DoctorAppointmentSystem.Model
{
    public class Appointment
    {
        public int Id { get; set; }

        public int SlotId { get; set; }
        public  AppointmentSlot? Slot { get; set; }

        public required string PatientId { get; set; }

        public AppointmentStatus Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

     
        public string? Notes { get; set; }
    }
}
