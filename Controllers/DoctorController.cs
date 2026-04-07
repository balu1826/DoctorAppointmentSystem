using DoctorAppointmentSystem.DTO;
using DoctorAppointmentSystem.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DoctorAppointmentSystem.Controllers
{
    [Authorize(Roles = "Doctor")]
    [ApiController]
    [Route("api/doctor")]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpPost("profile/verify")]
        public async Task<IActionResult> SubmitProfile(DoctorProfileDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            await _doctorService.SubmitDoctorProfileAsync(userId, dto);

            return Ok("Profile submitted for verification");
        }

        [HttpPut("profile/update")]
        public async Task<IActionResult> UpdateProfile(DoctorProfileDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            await _doctorService.UpdateDoctorProfileAsync(userId, dto);

            return Ok("Profile submitted for re-verification");
        }

        [HttpPost("availability")]
        public async Task<IActionResult> SetAvailability(DoctorAvailabilityDTO dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            await _doctorService.SetAvailabilityAsync(userId, dto);

            return Ok(new
            {
                message = "Availability set successfully"
            });
        }
        [HttpPost("generate-slots")]
        public async Task<IActionResult> GenerateSlots(GenerateSlotsDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) {
                return Unauthorized("User not authenticated");
            }

            await _doctorService.GenerateSlotsAsync(userId, dto.NumberOfDays);

            return Ok(new
            {
                message = "Slots generated successfully"
            });
        }
    }
}
