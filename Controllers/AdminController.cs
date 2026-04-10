using DoctorAppointmentSystem.DTO;
using DoctorAppointmentSystem.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorAppointmentSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // Approve doctor via notification
        [HttpPut("notification/{notificationId}/approve")]
        public async Task<IActionResult> ApproveDoctor(int notificationId)
        {
            await _adminService.ApproveDoctorAsync(notificationId);

            return Ok(new
            {
                message = "Doctor approved successfully"
            });
        }

        //  Reject doctor via notification
        [HttpPut("notification/{notificationId}/reject")]
        public async Task<IActionResult> RejectDoctor(int notificationId, [FromBody] RejectDoctorDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest("Rejection reason is required");

            await _adminService.RejectDoctorAsync(notificationId, dto);

            return Ok(new
            {
                message = "Doctor rejected successfully"
            });
        }

        [HttpGet("get/All/Appointments")]
        public async Task<AdminAppointmentsResponseDTO> GetAllAppointmentsAsync()
        {
            return await _adminService.GetAllAppointmentsAsync();
        }

        [HttpGet("get/All/Users")]
        public async Task<List<UserDTO>> GetAllUsersAsync()
        {
            return await _adminService.GetAllUsersAsync();
        }

        [HttpPut("users/{id}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var result = await _adminService.ToggleUserStatusAsync(id);

            if (result.Contains("not found"))
                return NotFound(result);

            return Ok(new { message = result });
        }
    }
}
