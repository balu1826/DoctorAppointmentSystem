namespace DoctorAppointmentSystem.Middleware
{
    using DoctorAppointmentSystem.DB;
    using DoctorAppointmentSystem.DTO;
    using DoctorAppointmentSystem.Exceptions;
    using DoctorAppointmentSystem.Model;
    using Microsoft.EntityFrameworkCore;
    using System.Net;
    using System.Security.Claims;
    using System.Text.Json;

    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
      

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
          
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                using (var scope = context.RequestServices.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var systemLog = new SystemLog
                    {
                        UserId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        ApiEndpoint = context.Request.Path,
                        HttpMethod = context.Request.Method,
                        ExceptionMessage = ex.Message,
                        StackTrace = ex.StackTrace,
                        CreatedAt = DateTime.UtcNow
                    };

                    db.SystemLogs.Add(systemLog);
                    await db.SaveChangesAsync();
                }

                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var response = new ApiResponse<string>
            {
                Success = false,
                Message = ex.Message,
                StatusCode = (int)HttpStatusCode.InternalServerError
            };

            switch (ex)
            {
                case BadRequestException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.StatusCode = 400;
                    break;

                case NotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.StatusCode = 404;
                    break;

                default:
                    context.Response.StatusCode = 500;
                    response.Message = "Something went wrong";
                    break;
            }

            context.Response.ContentType = "application/json";

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
    }
}
