using DoctorAppointmentSystem.DB;
using DoctorAppointmentSystem.DTO;
using DoctorAppointmentSystem.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DoctorAppointmentSystem.Controllers
{
    [ApiController]
    [Authorize(Roles = "Patient")]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpPost("book")]
        public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentDTO model)
        {
            //  Get patientId from JWT token
            var patientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!; ;

            if (string.IsNullOrEmpty(patientId))
                return Unauthorized("User not authenticated");

            var result = await _appointmentService.BookAppointmentAsync(patientId, model.slotId);

            //  Better response handling
            if (result.Contains("success"))
                return Ok(new { message = result });

            return BadRequest(new { message = result });
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var patientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(patientId))
                return Unauthorized();

            var result = await _appointmentService.CancelAppointmentAsync(id, patientId);

            return result.Contains("success") ? Ok(result) : BadRequest(result);
        }
        [HttpPut("{id}/reschedule")]
        public async Task<IActionResult> Reschedule(int id, [FromBody] int newSlotId)
        {
            var patientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(patientId))
                return Unauthorized();

            var result = await _appointmentService.RescheduleAppointmentAsync(id, newSlotId, patientId);

            return result.Contains("success") ? Ok(result) : BadRequest(result);
        }
    }
}