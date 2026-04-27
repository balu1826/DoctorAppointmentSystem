using DoctorAppointmentSystem.DTO;

namespace DoctorAppointmentSystem.Service.Interfaces
{
    public interface IDoctorService
    {
        Task SubmitDoctorProfileAsync( DoctorProfileDto dto);
        Task UpdateDoctorProfileAsync( DoctorProfileDto dto);
        Task SetAvailabilityAsync( DoctorAvailabilityDTO dto);
        Task GenerateSlotsAsync( int numberOfDays);
        Task<List<DoctorSlotDTO>> GetDoctorSlotsAsync();
        Task BlockSlotAsync(int slotId);
        Task UnblockSlotAsync(int slotId);
        Task<string> AcceptAppointmentAsync(int appointmentId);
        Task<string> RejectAppointmentAsync(int appointmentId);
        Task<string> CompleteAppointmentAsync(int appointmentId);
        Task<DoctorAppointmentsDTO> GetDoctorAppointmentsAsync();

    }
}
