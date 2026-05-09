using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Filters;
using MinimalAPIsMovies.Interfaces;
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

            group.MapGet("/renewtoken", Renew).RequireAuthorization();
            return group;
        }

        static async Task<Results<Ok<AuthenticationResponseDTO>,
            BadRequest<IEnumerable<IdentityError>>>> Register(UserCredentialsDTO userCredentialsDTO,
            [FromServices] UserManager<IdentityUser> userManager, IConfiguration configuration,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("UsersEndpoints");
            logger.LogInformation("Registering user with email: {Email}", userCredentialsDTO.Email);

            var user = new IdentityUser
            {
                UserName = userCredentialsDTO.Email,
                Email = userCredentialsDTO.Email
            };

            var result = await userManager.CreateAsync(user, userCredentialsDTO.Password);

            if (result.Succeeded)
            {
                logger.LogInformation("User registered successfully with email: {Email}", userCredentialsDTO.Email);
                var authenticationResponse = await BuildToken(userCredentialsDTO, userManager, configuration);
                return TypedResults.Ok(authenticationResponse);
            }
            else
            {
                logger.LogWarning("User registration failed for email: {Email}", userCredentialsDTO.Email);
                return TypedResults.BadRequest(result.Errors);
            }
        }

        static async Task<Results<Ok<AuthenticationResponseDTO>, BadRequest<string>>> Login(
            UserCredentialsDTO userCredentialsDTO, [FromServices] SignInManager<IdentityUser> signInManager,
            [FromServices] UserManager<IdentityUser> userManager, IConfiguration configuration,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("UsersEndpoints");
            logger.LogInformation("Login attempt for email: {Email}", userCredentialsDTO.Email);

            var user = await userManager.FindByEmailAsync(userCredentialsDTO.Email);

            if (user is null)
            {
                logger.LogWarning("Login failed - user not found for email: {Email}", userCredentialsDTO.Email);
                return TypedResults.BadRequest("There was a problem with the email or the password");
            }

            var result = await signInManager.CheckPasswordSignInAsync(user,
                userCredentialsDTO.Password, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                logger.LogInformation("User logged in successfully with email: {Email}", userCredentialsDTO.Email);
                var authenticationResponse = await BuildToken(userCredentialsDTO, userManager, configuration);
                return TypedResults.Ok(authenticationResponse);
            }
            else
            {
                logger.LogWarning("Login failed - invalid password for email: {Email}", userCredentialsDTO.Email);
                return TypedResults.BadRequest("There was a problem with the email or the password");
            }
        }

        static async Task<Results<NoContent, NotFound>> MakeAdmin(EditClaimDTO editClaimDTO,
            [FromServices] UserManager<IdentityUser> userManager, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("UsersEndpoints");
            logger.LogInformation("Granting admin claim to user with email: {Email}", editClaimDTO.Email);

            var user = await userManager.FindByEmailAsync(editClaimDTO.Email);

            if (user is null)
            {
                logger.LogWarning("User with email: {Email} was not found for admin grant", editClaimDTO.Email);
                return TypedResults.NotFound();
            }

            await userManager.AddClaimAsync(user, new Claim("isadmin", "true"));

            logger.LogInformation("Admin claim granted successfully to user with email: {Email}", editClaimDTO.Email);
            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound>> RemoveAdmin(EditClaimDTO editClaimDTO,
            [FromServices] UserManager<IdentityUser> userManager, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("UsersEndpoints");
            logger.LogInformation("Removing admin claim from user with email: {Email}", editClaimDTO.Email);

            var user = await userManager.FindByEmailAsync(editClaimDTO.Email);

            if (user is null)
            {
                logger.LogWarning("User with email: {Email} was not found for admin removal", editClaimDTO.Email);
                return TypedResults.NotFound();
            }

            await userManager.RemoveClaimAsync(user, new Claim("isadmin", "true"));

            logger.LogInformation("Admin claim removed successfully from user with email: {Email}", editClaimDTO.Email);
            return TypedResults.NoContent();
        }

        private static async Task<Results<Ok<AuthenticationResponseDTO>, NotFound>> Renew(
            IUsersService usersService, IConfiguration configuration,
            [FromServices] UserManager<IdentityUser> userManager, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("UsersEndpoints");
            logger.LogInformation("Renewing token for current user");

            var user = await usersService.GetUserAsync();

            if (user is null)
            {
                logger.LogWarning("Token renewal failed - current user was not found");
                return TypedResults.NotFound();
            }

            var userCredentials = new UserCredentialsDTO { Email = user.Email! };
            var response = await BuildToken(userCredentials, userManager, configuration);

            logger.LogInformation("Token renewed successfully for user with email: {Email}", user.Email);
            return TypedResults.Ok(response);
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