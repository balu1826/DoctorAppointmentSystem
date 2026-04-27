using DoctorAppointmentSystem.DTO;
using Microsoft.AspNetCore.Mvc;

namespace DoctorAppointmentSystem.Extensions
{
    public static class ControllerExtensions
    {
        public static IActionResult ApiOk<T>(this ControllerBase controller, T data, string message = "Success")
        {
            var response = new ApiResponse<T>
            {
                Status = StatusCodes.Status200OK,
                Success = true,
                Message = message,
                Data = data
            };

            return controller.Ok(response);
        }

        public static IActionResult ApiBadRequest(this ControllerBase controller, string message)
        {
            var response = new ApiResponse<object>
            {
                Status = StatusCodes.Status400BadRequest,
                Success = false,
                Message = message,
                Error = "Invalid Request"
            };

            return controller.BadRequest(response);
        }
    }
}
