using DoctorAppointmentSystem.DB;
using DoctorAppointmentSystem.DTO;
using DoctorAppointmentSystem.Service.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace DoctorAppointmentSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly AutoMapper.IMapper _mapper;
        private readonly AppDbContext _context;

        public AdminController(IAdminService adminService, AutoMapper.IMapper mapper, AppDbContext context)
        {
            _adminService = adminService;
            _mapper = mapper;
            _context = context;
        }
        [HttpGet("benchmark-users")]
        public async Task<IActionResult> BenchmarkUsers()
        {
            var users = await _context.Users.ToListAsync();

            var sw = Stopwatch.StartNew();

            // AutoMapper
            for (int i = 0; i < 1000; i++)
            {
                var auto = _mapper.Map<List<UserDTO>>(users);
            }

            sw.Stop();
            var autoTime = sw.ElapsedMilliseconds;

            sw.Restart();

            // Mapster
            for (int i = 0; i < 1000; i++)
            {
                var mapster = users.Adapt<List<UserDTO>>();
            }

            sw.Stop();
            var mapsterTime = sw.ElapsedMilliseconds;

            return Ok(new
            {
                AutoMapper = autoTime,
                Mapster = mapsterTime
            });
        }

        [HttpGet("memory-test")]
        public async Task<IActionResult> MemoryTest()
        {
            var users = await _context.Users.ToListAsync();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            long before = GC.GetTotalMemory(true);

            // AutoMapper
            var auto = _mapper.Map<List<DoctorProfileViewDTO>>(users);

            long afterAuto = GC.GetTotalMemory(true);

            // Mapster
            var mapster = users.Adapt<List<UserDTO>>();

            long afterMapster = GC.GetTotalMemory(true);

            return Ok(new
            {
                AutoMapperMemory = afterAuto - before,
                MapsterMemory = afterMapster - afterAuto
            });
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

        [HttpGet("Audit-logs")]
        public async Task<IActionResult> GetLogs()
        {
            var logs = await _adminService.GetAuditLogsAsync();
            return Ok(logs);
        }

        [HttpGet("system-logs")]
        public async Task<IActionResult> GetSystemLogs()
        {
            var logs = await _adminService.GetSystemLogsAsync();
            return Ok(logs);
        }
    }
}
