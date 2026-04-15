using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Filters;
using MinimalAPIsMovies.Utilities;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MinimalAPIsMovies.Endpoints
{
    public static class UsersEndpoints
    {
        public static RouteGroupBuilder MapUsers(this RouteGroupBuilder group)
        {
            group.MapPost("/", Register)
                .AddEndpointFilter<ValidationFilter<UserCredentialsDTO>>();
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

        private async static Task<AuthenticationResponseDTO> BuildToken(
            UserCredentialsDTO userCredentialsDTO, UserManager<IdentityUser> userManager,
            IConfiguration configuration)
        {
            var claims = new List<Claim>
            {
                new Claim("email", userCredentialsDTO.Email)
            };

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
