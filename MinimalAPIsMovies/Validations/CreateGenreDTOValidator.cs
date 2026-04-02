using FluentValidation;
using Microsoft.AspNetCore.Components.Routing;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace MinimalAPIsMovies.Validations
{
    public class CreateGenreDTOValidator : AbstractValidator<CreateGenreDTO>
    {
        public CreateGenreDTOValidator(IGenresRepository genresRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            var routeValueId = httpContextAccessor.HttpContext!.Request.RouteValues["id"];
            var id = 0;

            if (routeValueId is string routeValueIdString)
            {
                int.TryParse(routeValueIdString, out id);
            }

            RuleFor(p => p.Name)
                .NotEmpty()
                    .WithMessage("The field {PropertyName} is required")
                .MaximumLength(150)
                    .WithMessage("The field {PropertyName} should be less that {MaxLength} characters")
                .Must(FirstLetterIsUppercase)
                    .WithMessage("The field {PropertyName} should start with uppercase")
                .MustAsync(async (name, _) =>
                {
                    var exists = await genresRepository.ExistsAsync(id, name);
                    return !exists;
                }).WithMessage(g => $"A genre with the name {g.Name} already exists");
        }

        private bool FirstLetterIsUppercase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            var firstLetter = value[0].ToString();
            return firstLetter == firstLetter.ToUpper();
        }
    }
}
