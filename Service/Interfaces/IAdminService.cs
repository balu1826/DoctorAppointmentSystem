using DoctorAppointmentSystem.DTO;

namespace DoctorAppointmentSystem.Service.Interfaces
{
    public interface IAdminService
    {
        Task ApproveDoctorAsync(int notificationId);
        Task RejectDoctorAsync(int notificationId, RejectDoctorDto dto);
    }
}
