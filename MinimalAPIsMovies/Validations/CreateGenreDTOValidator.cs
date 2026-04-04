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
                    .WithMessage(ValidationUtilities.NonEmptyMessage)
                .MaximumLength(150)
                    .WithMessage(ValidationUtilities.MaximumLengthMessage)
                .Must(ValidationUtilities.FirstLetterIsUppercase)
                    .WithMessage(ValidationUtilities.FirstLetterIsUppercaseMessage)
                .MustAsync(async (name, _) =>
                {
                    var exists = await genresRepository.ExistsAsync(id, name);
                    return !exists;
                }).WithMessage(g => $"A genre with the name {g.Name} already exists");
        }
    }
}
