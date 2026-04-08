using DoctorAppointmentSystem.DB;
using DoctorAppointmentSystem.DTO;
using DoctorAppointmentSystem.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorAppointmentSystem.Controllers
{
    [ApiController]
    [Authorize("Patient")]
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
            var patientId = User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(patientId))
                return Unauthorized("User not authenticated");

            var result = await _appointmentService.BookAppointmentAsync(patientId, model.slotId);

            //  Better response handling
            if (result.Contains("success"))
                return Ok(new { message = result });

            return BadRequest(new { message = result });
        }
    }
}