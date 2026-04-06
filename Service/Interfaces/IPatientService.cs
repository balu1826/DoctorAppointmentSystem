using DoctorAppointmentSystem.DTO;

namespace DoctorAppointmentSystem.Service.Interfaces
{
    public interface IPatientService
    {
        Task<string> CompleteProfileAsync(string userId, PatientProfileDTO model);
        Task<string> UpdateProfileAsync(string userId, PatientProfileDTO model);
    }
}
