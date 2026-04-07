using DoctorAppointmentSystem.DTO;

namespace DoctorAppointmentSystem.Service.Interfaces
{
    public interface IDoctorService
    {
        Task SubmitDoctorProfileAsync(string userId, DoctorProfileDto dto);
        Task UpdateDoctorProfileAsync(string userId, DoctorProfileDto dto);
    }
}
