using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Filters;
using MinimalAPIsMovies.Interfaces;
using System.Runtime.CompilerServices;

namespace MinimalAPIsMovies.Endpoints
{
    public static class GenresEndpoints
    {
        public static RouteGroupBuilder MapGenres(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetGenres)
                .CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("genres-get"))
                .RequireAuthorization();
            group.MapGet("/{id:int}", GetById);
            group.MapPost("/", Create)
                .AddEndpointFilter<ValidationFilter<CreateGenreDTO>>()
                .RequireAuthorization("isadmin");
            group.MapPut("/{id:int}", Update)
                .AddEndpointFilter<ValidationFilter<CreateGenreDTO>>()
                .RequireAuthorization("isadmin");
            group.MapDelete("/{id:int}", Delete).RequireAuthorization("isadmin");
            return group;
        }

        static async Task<Ok<List<GenreDTO>>> GetGenres(IGenresRepository genresRepository,
            IMapper mapper, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("GenresEndpoints");
            logger.LogInformation("Fetching all genres");

            var genres = await genresRepository.GetAllAsync();
            var genreDTOs = mapper.Map<List<GenreDTO>>(genres);

            logger.LogInformation("Returning {Count} genres", genreDTOs.Count);
            return TypedResults.Ok(genreDTOs);
        }

        static async Task<Results<Ok<GenreDTO>, NotFound>> GetById(int id,
            IGenresRepository genresRepository, IMapper mapper, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("GenresEndpoints");
            logger.LogInformation("Fetching genre with id: {Id}", id);

            var genre = await genresRepository.GetByIdAsync(id);

            if (genre is null)
            {
                logger.LogWarning("Genre with id: {Id} was not found", id);
                return TypedResults.NotFound();
            }

            var genreDTO = mapper.Map<GenreDTO>(genre);

            logger.LogInformation("Returning genre with id: {Id}", id);
            return TypedResults.Ok(genreDTO);
        }

        static async Task<Created<GenreDTO>> Create(CreateGenreDTO createGenreDTO,
            IGenresRepository genresRepository, IOutputCacheStore outputCacheStore,
            IMapper mapper, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("GenresEndpoints");
            logger.LogInformation("Creating a new genre with name: {Name}", createGenreDTO.Name);

            var genre = mapper.Map<Genre>(createGenreDTO);
            var id = await genresRepository.CreateAsync(genre);
            await outputCacheStore.EvictByTagAsync("genres-get", default);
            var genreDTO = mapper.Map<GenreDTO>(genre);

            logger.LogInformation("Genre created successfully with id: {Id}", id);
            return TypedResults.Created($"/genres/{id}", genreDTO);
        }

        static async Task<Results<NotFound, NoContent>> Update(int id,
            CreateGenreDTO createGenreDTO, IGenresRepository repository,
            IOutputCacheStore outputCacheStore, IMapper mapper, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("GenresEndpoints");
            logger.LogInformation("Updating genre with id: {Id}", id);

            var exists = await repository.ExistsAsync(id);

            if (!exists)
            {
                logger.LogWarning("Genre with id: {Id} was not found for update", id);
                return TypedResults.NotFound();
            }

            var genre = mapper.Map<Genre>(createGenreDTO);
            genre.Id = id;
            await repository.UpdateAsync(genre);
            await outputCacheStore.EvictByTagAsync("genres-get", default);

            logger.LogInformation("Genre with id: {Id} updated successfully", id);
            return TypedResults.NoContent();
        }

        static async Task<Results<NotFound, NoContent>> Delete(int id, IGenresRepository genresRepository,
            IOutputCacheStore outputCacheStore, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("GenresEndpoints");
            logger.LogInformation("Deleting genre with id: {Id}", id);

            var exists = await genresRepository.ExistsAsync(id);

            if (!exists)
            {
                logger.LogWarning("Genre with id: {Id} was not found for deletion", id);
                return TypedResults.NotFound();
            }

            await genresRepository.DeleteAsync(id);
            await outputCacheStore.EvictByTagAsync("genres-get", default);

            logger.LogInformation("Genre with id: {Id} deleted successfully", id);
            return TypedResults.NoContent();
        }
    }
}