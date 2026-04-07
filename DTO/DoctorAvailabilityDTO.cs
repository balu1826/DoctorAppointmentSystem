using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentSystem.DTO
{
    public class DoctorAvailabilityDTO
    {
        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Range(5, 120)]
        public int SlotDurationInMinutes { get; set; }
    }
}
