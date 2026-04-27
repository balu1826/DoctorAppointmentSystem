using DoctorAppointmentSystem.Attributes;
using DoctorAppointmentSystem.DTO;
using DoctorAppointmentSystem.Exceptions;
using DoctorAppointmentSystem.Extensions;
using DoctorAppointmentSystem.Filters;
using DoctorAppointmentSystem.Service;
using DoctorAppointmentSystem.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static System.Reflection.Metadata.BlobBuilder;

namespace DoctorAppointmentSystem.Controllers
{
    [ServiceFilter(typeof(DoctorAccessFilter))]
    [DoctorAccess]
    [ApiController]
    [Route("api/doctor")]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }
        //Profile Verification
        [AuditLog(
        action: "Doctor Verification Request",
        entityType: "Doctor",
        referenceIdParam: "doctorId",
        description: "Doctor requested Verification"
        )]
        [HttpPost("profile/verify")]
        public async Task<IActionResult> SubmitProfile(DoctorProfileDto dto)
        {
           
            await _doctorService.SubmitDoctorProfileAsync( dto);
            return this.ApiOk("Profile submitted for verification");
        }
        //Profile Re-Verification
        [AuditLog(
        action: "Doctor Re-Verification Request",
        entityType: "Doctor",
        referenceIdParam: "doctorId",
        description: "Doctor requested re-verification"
        )]
        [HttpPut("profile/update")]
        public async Task<IActionResult> UpdateProfile(DoctorProfileDto dto)
        {
  
            await _doctorService.UpdateDoctorProfileAsync( dto);
            return this.ApiOk("Profile submitted for re-verification");
        }
       //Set Availability
       [AuditLog(
       action: "Doctor Set Availability",
       entityType: "Doctor",
       referenceIdParam: "doctorId",
       description: "Doctor successfully set availability!"
       )]
        [HttpPost("availability")]
        public async Task<IActionResult> SetAvailability(DoctorAvailabilityDTO dto)
        {
           
            await _doctorService.SetAvailabilityAsync( dto);
            return this.ApiOk( "Availability set successfully");
        }
       //Generated Slots
       [AuditLog(
       action: "Doctor Generated slots",
       entityType: "Slots",
       referenceIdParam: "",
       description: "Doctor Successfully Generated Slots"
       )]
        [HttpPost("generate-slots")]
        public async Task<IActionResult> GenerateSlots(GenerateSlotsDto dto)
        {
            await _doctorService.GenerateSlotsAsync( dto.NumberOfDays);
            return this.ApiOk("Slots generated successfully");
        }
        // Get Slots
        [HttpGet("slots")]
        public async Task<IActionResult> GetSlots()
        {
            var slots = await _doctorService.GetDoctorSlotsAsync();
            return this.ApiOk(slots,"Slots generated successfully");
        }

        // Block Slot
        [AuditLog(
       action: "Doctor Blocks Slots",
       entityType: "Slots",
       referenceIdParam: "slotId",
       description: "Doctor Blocked Slot Successfully"
       )]
        [HttpPut("slots/{slotId}/block")]
        public async Task<IActionResult> BlockSlot(int slotId)
        {
            await _doctorService.BlockSlotAsync( slotId);
            return this.ApiOk("Slots Booked successfully");
        }
       //  Unblock Slot
       [AuditLog(
       action: "Doctor UnBlocks Slots",
       entityType: "Slots",
       referenceIdParam: "slotId",
       description: "Doctor UnBlocked Slot Successfully"
       )]
        [HttpPut("slots/{slotId}/unblock")]
        public async Task<IActionResult> UnblockSlot(int slotId)
        {
            await _doctorService.UnblockSlotAsync( slotId);
            return this.ApiOk("Slot UnBlocked successfully");
        }
        //Accept Appointment
        [AuditLog(
        action: "Doctor Accepted Appointment",
        entityType: "Appointment",
        referenceIdParam: "id",
        description: "Doctor Accepted Appointment Successfully!"
        )]
        [HttpPut("{id}/accept")]
        public async Task<IActionResult> Accept(int id)
        {
            var result = await _doctorService.AcceptAppointmentAsync(id);
            return this.ApiOk("Appointment Accepted successfully");
        }
       //Appointment Rejected
       [AuditLog(
       action: "Doctor Rejected Appointment",
       entityType: "Appointment",
       referenceIdParam: "id",
       description: "Doctor Rejected Appointment Successfully!"
       )]
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var result = await _doctorService.RejectAppointmentAsync(id);
            return this.ApiOk("Appointment Rejected");
        }
       //Appointment Completed
       [AuditLog(
       action: "Appointment Completed",
       entityType: "Appointment",
       referenceIdParam: "id",
       description: "Doctor completed Appointment Successfully!"
       )]
        [HttpPut("{id}/complete")]
        public async Task<IActionResult> Complete(int id)
        {
            var result = await _doctorService.CompleteAppointmentAsync(id);
            return this.ApiOk("Appointment Booked successfully");
        }
        //Get Appointments
        [HttpGet("doctor/appointments")]
        public async Task<IActionResult> GetDoctorAppointments()
        {
            var result = await _doctorService.GetDoctorAppointmentsAsync();
            return this.ApiOk(result,"Doctor Appointments");
        }
    }
}
