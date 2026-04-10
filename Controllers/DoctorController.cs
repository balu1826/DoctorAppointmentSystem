using DoctorAppointmentSystem.DTO;
using DoctorAppointmentSystem.Exceptions;
using DoctorAppointmentSystem.Service;
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
            try
            {
                await _doctorService.UpdateDoctorProfileAsync(userId, dto);
            }
            catch(BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Something went wrong" });
            }
            

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

        // Get Slots
        [HttpGet("slots")]
        public async Task<IActionResult> GetSlots()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var slots = await _doctorService.GetDoctorSlotsAsync(userId);

            return Ok(slots);
        }

        // Block Slot
        [HttpPut("slots/{slotId}/block")]
        public async Task<IActionResult> BlockSlot(int slotId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            try
            {
                await _doctorService.BlockSlotAsync(userId, slotId);

                return Ok(new { message = "Slot blocked successfully" });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message }); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Something went wrong" });
            }
        }

        //  Unblock Slot
        [HttpPut("slots/{slotId}/unblock")]
        public async Task<IActionResult> UnblockSlot(int slotId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }
            try
            {

                await _doctorService.UnblockSlotAsync(userId, slotId);

                return Ok(new { message = "Slot unblocked successfully" });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Something went wrong" });

            }
        }

        [HttpPut("{id}/accept")]
        public async Task<IActionResult> Accept(int id)
        {
            var doctorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
            {
                return Unauthorized("User not authenticated");
            }

            var result = await _doctorService.AcceptAppointmentAsync(id, doctorId);

            return result.Contains("accepted") ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var doctorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
            {
                return Unauthorized("User not authenticated");
            }
            var result = await _doctorService.RejectAppointmentAsync(id, doctorId);

            return result.Contains("rejected") ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}/complete")]
        public async Task<IActionResult> Complete(int id)
        {
            var doctorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
            {
                return Unauthorized("User not authenticated");
            }

            var result = await _doctorService.CompleteAppointmentAsync(id, doctorId);

            return result.Contains("completed") ? Ok(result) : BadRequest(result);
        }

        [HttpGet("doctor/appointments")]
        public async Task<IActionResult> GetDoctorAppointments()
        {
            var doctorUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(doctorUserId))
                return Unauthorized();

            var result = await _doctorService.GetDoctorAppointmentsAsync(doctorUserId);

            return Ok(result);
        }
    }
}
