using DoctorAppointmentSystem.DTO;
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
            var result = await _patientService.CompleteProfileAsync(GetUserId(), model);
            return Ok(result);
        }

        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile(PatientProfileDTO model)
        {
            var result = await _patientService.UpdateProfileAsync(GetUserId(), model);
            return Ok(result);
        }
    }
}
