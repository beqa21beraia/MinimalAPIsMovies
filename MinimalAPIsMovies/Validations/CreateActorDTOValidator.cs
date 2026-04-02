using FluentValidation;
using MinimalAPIsMovies.DTOs;

namespace MinimalAPIsMovies.Validations
{
    public class CreateActorDTOValidator : AbstractValidator<CreateActorDTO>
    {
        public CreateActorDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                    .WithMessage("The field {PropertyName} is required")
                .MaximumLength(150)
                    .WithMessage("The field {PropertyName} should be less that {MaxLength} characters");

            var minimumDate = new DateTime(1900, 1, 1);

            RuleFor(x => x.DateOfBirth)
                .GreaterThanOrEqualTo(minimumDate)
                    .WithMessage("The field {PropertyName} should be greater than " + minimumDate.ToString("yyyy-MM-dd"));
        }
    }
}
