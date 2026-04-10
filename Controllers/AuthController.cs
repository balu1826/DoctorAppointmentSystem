using DoctorAppointmentSystem.DB;
using DoctorAppointmentSystem.DTO;
using DoctorAppointmentSystem.Service.Interfaces;
using DoctorAppointmentSystem.Exceptions;
using DoctorAppointmentSystem.Model;
using DoctorAppointmentSystem.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DoctorAppointmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController :  ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAdminService _adminService;
        public AuthController(UserManager<ApplicationUser> userManager,
                      SignInManager<ApplicationUser> signInManager,
                      RoleManager<IdentityRole> roleManager,
                      AppDbContext context,
                      TokenService tokenService,
                      IAdminService adminService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _signInManager = signInManager;
            _adminService = adminService;
            _tokenService = tokenService;

        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest model)
        {
            // Check user exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return BadRequest("User already exists");

            // Create Identity user
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var role = model.Role.ToString();

            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

            // Assign role
            await _userManager.AddToRoleAsync(user, role);

            //  Create domain entity
            if (role == "Doctor")
            {
                var doctor = new Doctor
                {
                    UserId = user.Id,
                    Specialization = "General", // default (update later)
                    ExperienceYears = 0
                };

                _context.Doctors.Add(doctor);
            }
            else if (role == "Patient")
            {
                var patient = new Patient
                {
                    UserId = user.Id,
                    BloodGroup = "Unknown"
                };

                _context.Patients.Add(patient);
            }
            await _adminService.LogAsync(
           "New Register",
            user.Id,
            "ApplicationUser"
       );
            await _context.SaveChangesAsync();

            return Ok("User registered successfully");
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new BadRequestException("Invalid credentials");
            if (!user.IsActive)
                throw new BadRequestException("User account is inactive");

            var accessToken =await _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

            _context.RefreshTokens.Add(refreshToken);
            await _adminService.LogAsync(
                "Login",
                 user.Id,
                 "RefreshToken"
            );
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Login successful",
                StatusCode = 200,
                Data = new
                {
                    accessToken,
                    refreshToken = refreshToken.Token
                }
            });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(userId == null)
                return BadRequest("Invalid user");
            var tokens = await _context.RefreshTokens
                .Where(x => x.UserId == userId)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }
            await _adminService.LogAsync(
             "Logout",
             userId,
             "ApplicationUser"
              );
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Logged out successfully",
                StatusCode = 200
            });
        }


        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == refreshToken);

            if (token == null || token.IsRevoked || token.ExpiryDate < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Invalid refresh token");

            var user = await _userManager.FindByIdAsync(token.UserId);
            if(user == null)
                throw new UnauthorizedAccessException("User not found");

            var newAccessToken = _tokenService.GenerateAccessToken(user);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Token refreshed",
                StatusCode = 200,
                Data = new { accessToken = newAccessToken }
            });
        }

      
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            // Do NOT reveal if user exists (security)
            if (user == null)
                return Ok("Your account is not found");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            await _adminService.LogAsync(
                "Forgot Password",
                user.Id,
                "ApplicationUser",
                null,
                "User requested password reset"
            );
            await _context.SaveChangesAsync();
            // return token
            return Ok(new
            {
                Message = "Reset token generated",
                Token = token
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return BadRequest("Invalid request");

            var result = await _userManager.ResetPasswordAsync(
                user,
                model.Token,
                model.NewPassword
            );

            if (!result.Succeeded)
                return BadRequest(result.Errors);
            await _adminService.LogAsync(
                "Password Reset",
                user.Id,
                "ApplicationUser",
                null,
                "User reset password successfully"
            );
            await _context.SaveChangesAsync();

            return Ok("Password reset successful");
        }
    }
}
