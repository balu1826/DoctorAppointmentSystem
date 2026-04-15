using DoctorAppointmentSystem.DTO;
using DoctorAppointmentSystem.Service;
using DoctorAppointmentSystem.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DoctorAppointmentSystem.Controllers
{
    [ApiController]
    [Route("api/patient")]
    [Authorize(Roles = "Patient")]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        }

        [HttpPost("complete-profile")]
        public async Task<IActionResult> CompleteProfile(PatientProfileDTO model)
        {
            var result = await _patientService.SavePatientProfileAsync(
        GetUserId(),
        model,
        requireVerified: false,
        logMessage: "Profile Verified",
        successMessage: "Your account verified successfully!");
            return Ok(result);
        }

        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile(PatientProfileDTO model)
        {
            var result = await _patientService.SavePatientProfileAsync(
        GetUserId(),
        model,
        requireVerified: true,
        logMessage: "Profile Updated",
        successMessage: "Profile updated successfully!");
            return Ok(result);
        }

        [HttpGet("get-All-Doctors")]
        public async Task<IActionResult> GetDoctors()
        {
            var doctors = await _patientService.GetDoctorsAsync();
            return Ok(doctors);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchDoctors([FromQuery] DoctorFilterDTO filter)
        {
            var result = await _patientService.SearchDoctorsAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDoctor(int id)
        {
            var doctor = await _patientService.GetDoctorByIdAsync(id);

            if (doctor == null)
                return NotFound("Doctor not found");

            return Ok(doctor);
        }

        [HttpGet("my/appointments")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var patientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(patientId))
                return Unauthorized();

            var result = await _patientService.GetPatientAppointmentsAsync(patientId);

            return Ok(result);
        }
    }
}
