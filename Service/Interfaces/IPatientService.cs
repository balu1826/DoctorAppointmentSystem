using DoctorAppointmentSystem.DTO;

namespace DoctorAppointmentSystem.Service.Interfaces
{
    public interface IPatientService
    {
        Task<string> CompleteProfileAsync(string userId, PatientProfileDTO model);
        Task<string> UpdateProfileAsync(string userId, PatientProfileDTO model);
        Task<List<DoctorListDTO>> GetDoctorsAsync();
        Task<List<DoctorListDTO>> SearchDoctorsAsync(DoctorFilterDTO filter);
        Task<DoctorProfileViewDTO?> GetDoctorByIdAsync(int id);
        Task<PatientAppointmentsDTO> GetPatientAppointmentsAsync(string patientId);

    }
}
