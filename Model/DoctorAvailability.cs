namespace DoctorAppointmentSystem.Model
{
    public class DoctorAvailability
    {
        public int Id { get; set; }

        public int DoctorId { get; set; }
        public virtual Doctor? Doctor { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public int SlotDurationInMinutes { get; set; }

    }
}
