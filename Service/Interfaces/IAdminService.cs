using DoctorAppointmentSystem.DTO;

namespace DoctorAppointmentSystem.Service.Interfaces
{
    public interface IAdminService
    {
        Task ApproveDoctorAsync(int notificationId);
        Task RejectDoctorAsync(int notificationId, RejectDoctorDto dto);
        Task<AdminAppointmentsResponseDTO> GetAllAppointmentsAsync();
        Task<List<UserDTO>> GetAllUsersAsync();
        Task<string> ToggleUserStatusAsync(string userId);
        Task LogAsync(string action, string userId, string entityType, int? referenceId=null, string? description = null);
        Task<List<AuditLogDTO>> GetAuditLogsAsync();
        Task<List<SystemLogDTO>> GetSystemLogsAsync();
    }
}
