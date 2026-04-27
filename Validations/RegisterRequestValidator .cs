using FluentValidation;
using DoctorAppointmentSystem.DTO;

namespace DoctorAppointmentSystem.Validations
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(9)
                .Matches(@"^(?:.*[A-Z]){2,}(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&]).+$")
                .WithMessage("Password must contain at least 2 uppercase letters, lowercase letters, numbers, and special characters");

            RuleFor(x => x.FullName)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(100);

            RuleFor(x => x.Role)
                .IsInEnum();
        }
    }
}
