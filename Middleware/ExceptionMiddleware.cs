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
            var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
            var response = new ApiResponse<object>
            {
                Success = false,
                Message = "Something went wrong",
                Errors = new List<string>()
            };

            switch (ex)
            {
                case BadRequestException:
                    context.Response.StatusCode = 400;
                    response.Status = 400;
                    response.Message = ex.Message;
                    response.Error = "Invalid Request";
                    break;

                case NotFoundException:
                    context.Response.StatusCode = 404;
                    response.Status = 404;
                    response.Message = ex.Message;
                    response.Error = "Resource Not Found";
                    break;
                case UnauthorizedAccessException:
                    context.Response.StatusCode = 401;
                    response.Status = 401;
                    response.Message = "Unauthorized";
                    response.Error = "Invalid Authorization";
                    break;
                case ForbiddenAccessException:
                    context.Response.StatusCode = 403;
                    response.Status = 403;
                    response.Message = "Access Denied";
                    response.Error = "Invalid Access";
                    break; 

                default:
                    context.Response.StatusCode = 500;
                    response.Status = 500;
                    response.Errors.Add(ex.Message);
                    response.Error = "Internal Server Error";
                    break;
            }
          
            if (env.IsDevelopment())
            {
                response.Errors.Add(ex.Message);
            }

            context.Response.ContentType = "application/json";

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
    }
}
