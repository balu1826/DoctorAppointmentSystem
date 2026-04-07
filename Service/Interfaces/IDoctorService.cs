using DoctorAppointmentSystem.DTO;

namespace DoctorAppointmentSystem.Service.Interfaces
{
    public interface IDoctorService
    {
        Task SubmitDoctorProfileAsync(string userId, DoctorProfileDto dto);
        Task UpdateDoctorProfileAsync(string userId, DoctorProfileDto dto);
        Task SetAvailabilityAsync(string userId, DoctorAvailabilityDTO dto);
        Task GenerateSlotsAsync(string userId, int numberOfDays);
    }
}
