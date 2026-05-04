using DoctorAppointmentSystem.DTO;
using DoctorAppointmentSystem.Model;
using DoctorAppointmentSystem.Pagination.AuditLog;

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
        Task<PagedResult<AuditLogDTO>> GetAuditLogsAsync(AuditLogQueryParams  queryParams);
        Task<List<SystemLogDTO>> GetSystemLogsAsync();
    }
}
