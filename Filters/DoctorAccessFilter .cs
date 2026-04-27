using DoctorAppointmentSystem.Attributes;
using DoctorAppointmentSystem.DB;
using DoctorAppointmentSystem.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DoctorAppointmentSystem.Filters
{
    public class DoctorAccessFilter : IAsyncActionFilter
    {
        private readonly AppDbContext _context;

        public DoctorAccessFilter(AppDbContext context)
        {
            _context = context;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
           
            var hasAttribute = context.ActionDescriptor.EndpointMetadata
                .OfType<DoctorAccessAttribute>()
                .Any();

            if (!hasAttribute)
            {
                await next();
                return;
            }


            var user = context.HttpContext.User;

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException("Unautorized Access!");
            }

        
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                throw new ForbiddenAccessException("Access Denied");
            }

        
            if (context.ActionArguments.TryGetValue("doctorId", out var value) && value is int requestedDoctorId)
            {
                if (doctor.Id != requestedDoctorId)
                {
                    throw new ForbiddenAccessException("Access Denied");
                }
            }

         
            await next();
        }
    }
}
