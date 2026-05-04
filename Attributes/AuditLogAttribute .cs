using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using DoctorAppointmentSystem.Service.Interfaces;
using DoctorAppointmentSystem.DB;

namespace DoctorAppointmentSystem.Attributes
{
    
    public class AuditLogAttribute : TypeFilterAttribute
    {
        public AuditLogAttribute(
            string action,
            string entityType,
            string referenceIdParam,
            string description)
            : base(typeof(AuditLogFilter))
        {
            Arguments = new object[]
            {
                action, entityType, referenceIdParam, description
            };
        }
    }

   
    public class AuditLogFilter : IAsyncActionFilter
    {
        private readonly IAdminService _adminService;
        private readonly AppDbContext _context;

        private readonly string _action;
        private readonly string _entityType;
        private readonly string _referenceIdParam;
        private readonly string _description;

        public AuditLogFilter(
            IAdminService adminService,
            AppDbContext context,
            string action,
            string entityType,
            string referenceIdParam,
            string description)
        {
            _adminService = adminService;
            _context = context;
            _action = action;
            _entityType = entityType;
            _referenceIdParam = referenceIdParam;
            _description = description;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            // Execute controller first
            var resultContext = await next();

            // Skip if exception occurred
            if (resultContext.Exception != null) return;

            //  Get UserId
            var userId = context.HttpContext.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User is not authenticated.");
            //  Extract ReferenceId
            string? referenceId = null;

            if (!string.IsNullOrEmpty(_referenceIdParam))
            {
                // Case 1: direct parameter
                if (context.ActionArguments.TryGetValue(_referenceIdParam, out var value))
                {
                    referenceId = value?.ToString();
                }
                else
                {
                    // Case 2: inside DTO
                    foreach (var arg in context.ActionArguments.Values)
                    {
                        if (arg == null) continue;

                        var prop = arg.GetType().GetProperty(_referenceIdParam);

                        if (prop != null)
                        {
                            referenceId = prop.GetValue(arg)?.ToString();
                            break;
                        }
                    }
                }
            }

            //  Save Audit Log
            await _adminService.LogAsync(
                _action,
                userId,
                _entityType,
                referenceId != null ? int.Parse(referenceId) : (int?)null,
                _description
            );

            await _context.SaveChangesAsync();
        }
    }
}