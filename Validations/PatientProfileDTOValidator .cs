using DoctorAppointmentSystem.DTO;
using FluentValidation;

namespace DoctorAppointmentSystem.Validations
{

    public class PatientProfileDTOValidator : AbstractValidator<PatientProfileDTO>
    {
        public PatientProfileDTOValidator()
        {
            RuleFor(x => x.BloodGroup)
                .NotEmpty()
                .Matches("^(A|B|AB|O)[+-]$")
                .WithMessage("INVALID blood group");

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.UtcNow)
                .WithMessage("INVALID date of birth");      
            RuleFor(x => x.Gender)
                .IsInEnum()
                .WithMessage("INVALID  gender");
        }
    }
}
