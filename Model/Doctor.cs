using DoctorAppointmentSystem.Model.Enums;

namespace DoctorAppointmentSystem.Model
{
    public class Doctor
    {
        public int Id { get; set; }

        public required string UserId { get; set; }
        public  ApplicationUser? User { get; set; }

        public required string Specialization { get; set; }
        public int ExperienceYears { get; set; }
        public bool IsApproved { get; set; } = false;

        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
    }
}
