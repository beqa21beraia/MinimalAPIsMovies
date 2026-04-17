using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Filters;
using MinimalAPIsMovies.Utilities;
using MinimalAPIsMovies.Validations;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MinimalAPIsMovies.Endpoints
{
    public static class UsersEndpoints
    {
        public static RouteGroupBuilder MapUsers(this RouteGroupBuilder group)
        {
            group.MapPost("/register", Register)
                .AddEndpointFilter<ValidationFilter<UserCredentialsDTO>>();
            group.MapPost("/login", Login)
                .AddEndpointFilter<ValidationFilter<UserCredentialsDTO>>();

            group.MapPost("/makeadmin", MakeAdmin)
                .AddEndpointFilter<ValidationFilter<EditClaimDTO>>()
                .RequireAuthorization("isadmin");

            group.MapPost("/removeadmin", RemoveAdmin)
                .AddEndpointFilter<ValidationFilter<EditClaimDTO>>()
                .RequireAuthorization("isadmin");
            return group;
        }

        static async Task<Results<Ok<AuthenticationResponseDTO>, 
            BadRequest<IEnumerable<IdentityError>>>> Register(UserCredentialsDTO userCredentialsDTO,
            [FromServices] UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            var user = new IdentityUser
            {
                UserName = userCredentialsDTO.Email,
                Email = userCredentialsDTO.Email
            };

            var result = await userManager.CreateAsync(user, userCredentialsDTO.Password);

            if (result.Succeeded)
            {
                var authenticationResponse = await BuildToken(userCredentialsDTO, userManager, configuration);
                return TypedResults.Ok(authenticationResponse);
            }
            else
            {
                return TypedResults.BadRequest(result.Errors);
            }
        }

        static async Task<Results<Ok<AuthenticationResponseDTO>, BadRequest<string>>> Login(
            UserCredentialsDTO userCredentialsDTO, [FromServices] SignInManager<IdentityUser> signInManager,
            [FromServices] UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            var user = await userManager.FindByEmailAsync(userCredentialsDTO.Email);

            if (user is null)
            {
                return TypedResults.BadRequest("There was a problem with the email or the password");
            }

            var result = await signInManager.CheckPasswordSignInAsync(user,
                userCredentialsDTO.Password, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var authenticationResponse = await BuildToken(userCredentialsDTO, userManager, configuration);
                return TypedResults.Ok(authenticationResponse);
            }
            else
            {
                return TypedResults.BadRequest("There was a problem with the email or the password");
            }
        }

        static async Task<Results<NoContent, NotFound>> MakeAdmin(EditClaimDTO editClaimDTO,
            [FromServices] UserManager<IdentityUser> userManager)
        {
            var user = await userManager.FindByEmailAsync(editClaimDTO.Email);

            if (user is null)
            {
                return TypedResults.NotFound();
            }

            await userManager.AddClaimAsync(user, new Claim("isadmin", "true"));
            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound>> RemoveAdmin(EditClaimDTO editClaimDTO,
            [FromServices] UserManager<IdentityUser> userManager)
        {
            var user = await userManager.FindByEmailAsync(editClaimDTO.Email);

            if (user is null)
            {
                return TypedResults.NotFound();
            }

            await userManager.RemoveClaimAsync(user, new Claim("isadmin", "true"));
            return TypedResults.NoContent();
        }

        private async static Task<AuthenticationResponseDTO> BuildToken(
            UserCredentialsDTO userCredentialsDTO, UserManager<IdentityUser> userManager,
            IConfiguration configuration)
        {
            var claims = new List<Claim>
            {
                new Claim("email", userCredentialsDTO.Email)
            };

            var user = await userManager.FindByNameAsync(userCredentialsDTO.Email);
            var claimsFromDB = await userManager.GetClaimsAsync(user!);

            claims.AddRange(claimsFromDB);

            var key = KeysHandler.GetKey(configuration).First();
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddYears(1);

            var securityToken = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiration,
                signingCredentials: credentials);

            var token = new JwtSecurityTokenHandler().WriteToken(securityToken);

            return new AuthenticationResponseDTO
            {
                Token = token,
                Expiration = expiration,
            };
        }
    }
}
