using DoctorAppointmentSystem.DTO;

namespace DoctorAppointmentSystem.Service.Interfaces
{
    public interface IPatientService
    {
        Task<string> SavePatientProfileAsync(
            string userId,
            PatientProfileDTO model,
            bool requireVerified,
            string logMessage,
            string successMessage);
   
        Task<List<DoctorListDTO>> GetDoctorsAsync();
        Task<List<DoctorListDTO>> SearchDoctorsAsync(DoctorFilterDTO filter);
        Task<DoctorProfileViewDTO?> GetDoctorByIdAsync(int id);
        Task<PatientAppointmentsDTO> GetPatientAppointmentsAsync(string patientId);

    }
}
