using DoctorAppointmentSystem.Util.Interfaces;
using System.Security.Claims;

namespace DoctorAppointmentSystem.Util.Implementations
{
    public class CurrentUserService : ICurrentUserService
    {

        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string UserId =>
     _httpContextAccessor.HttpContext?.User?
         .FindFirst(ClaimTypes.NameIdentifier)?.Value
     ?? throw new UnauthorizedAccessException("User not authenticated");
        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
}
