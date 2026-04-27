using DoctorAppointmentSystem.Attributes;
using DoctorAppointmentSystem.Model;
using DoctorAppointmentSystem.Service.Interfaces;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using DoctorAppointmentSystem.DB;

namespace DoctorAppointmentSystem.Filters
{
    public class AuditLogFilter : IAsyncActionFilter
    {
        private readonly IAdminService _adminService;
        private readonly AppDbContext _context;

        public AuditLogFilter(IAdminService adminService, AppDbContext context)
        {
            _adminService = adminService;
            _context = context;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();
            if (resultContext.Exception != null) return;

            var attribute = context.ActionDescriptor.EndpointMetadata
                         .OfType<AuditLogAttribute>()
                         .FirstOrDefault();

            if (attribute == null) return;

            // Get UserId
            var userId = context.HttpContext.User
                .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if(string.IsNullOrEmpty(userId)) 
                throw new UnauthorizedAccessException("User is not authenticated.");

            //  Get ReferenceId dynamically
            string? referenceId = null;

            if (!string.IsNullOrEmpty(attribute.ReferenceIdParam) &&
                context.ActionArguments.TryGetValue(attribute.ReferenceIdParam, out var value))
            {
                referenceId = value?.ToString();
            }
            await _adminService.LogAsync(attribute.Action,
                userId,
                attribute.EntityType,
                referenceId != null ? int.Parse(referenceId) : (int?)null,
                attribute.Description);
            await _context.SaveChangesAsync();
        }
    }
}
