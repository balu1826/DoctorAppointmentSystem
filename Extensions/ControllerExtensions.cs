using DoctorAppointmentSystem.DTO;
using Microsoft.AspNetCore.Mvc;

namespace DoctorAppointmentSystem.Extensions
{
    public static class ControllerExtensions
    {
        
        //For successsful API respose
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
        //For creating new resource
        public static IActionResult ApiCreated(this ControllerBase controller, string message)
        {
            var response = new ApiResponse<object>
            {
                Status = StatusCodes.Status201Created,
                Success = true,
                Message = message
            };
            return controller.StatusCode(StatusCodes.Status201Created, response);
        }
    }
}
