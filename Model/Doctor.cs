using DoctorAppointmentSystem.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentSystem.Model
{
    public class Doctor
    {
        public int Id { get; set; }

        public required string UserId { get; set; }
        public  ApplicationUser? User { get; set; }

        public required string Specialization { get; set; }
        public int ExperienceYears { get; set; }
        [Required]
        [Range(0, 100000, ErrorMessage = "Consultation fee must be between 0 and 100000")]
        public decimal ConsultationFee { get; set; }
        public bool IsApproved { get; set; } = false;

        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;

        public virtual ICollection<DoctorAvailability> Availabilities { get; set; } = new List<DoctorAvailability>();
        public ICollection<AppointmentSlot>? AppointmentSlot { get; set; }
    }
}
