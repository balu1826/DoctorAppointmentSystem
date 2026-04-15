using DoctorAppointmentSystem.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentSystem.DTO
{
    public class PatientProfileDTO
    {

        public required string BloodGroup { get; set; }

        public DateTime DateOfBirth { get; set; }

        public Gender Gender { get; set; }
    }
}
