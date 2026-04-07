namespace DoctorAppointmentSystem.Model
{
    public class AppointmentSlot
    {
        public int Id { get; set; }

        public int DoctorId { get; set; }
        public virtual Doctor? Doctor { get; set; }

        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

        public bool IsBooked { get; set; } = false;
        public bool IsBlocked { get; set; } = false;
    }
}
