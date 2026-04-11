using DoctorAppointmentSystem.DB;
using System.Security.Claims;
using DoctorAppointmentSystem.Model;

namespace DoctorAppointmentSystem.Middleware
{
    public class ActivityLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public ActivityLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            using var scope = context.RequestServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var log = new SystemLog
            {
                UserId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                HttpMethod = context.Request.Method,
               ApiEndpoint = context.Request.Path,
                CreatedAt = DateTime.UtcNow
            };

            await _next(context);

          

            db.SystemLogs.Add(log);
            await db.SaveChangesAsync();
        }
    }
}
