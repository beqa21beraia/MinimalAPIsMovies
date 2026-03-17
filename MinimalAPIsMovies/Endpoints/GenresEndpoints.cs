using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Interfaces;
using System.Runtime.CompilerServices;

namespace MinimalAPIsMovies.Endpoints
{
    public static class GenresEndpoints
    {
        public static RouteGroupBuilder MapGenres(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetGenres)
                .CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("genres-get"));
            group.MapGet("/{id:int}", GetById);
            group.MapPost("/", Create);
            group.MapPut("/{id:int}", Update);
            group.MapDelete("/{id:int}", Delete);

            return group;
        }

        static async Task<Ok<List<GenreDTO>>> GetGenres(IGenresRepository genresRepository)
        {
            var genres = await genresRepository.GetAllAsync();

            var genreDTOs = genres.Select(g => new GenreDTO { Id = g.Id, Name = g.Name }).ToList();

            return TypedResults.Ok(genreDTOs);
        }

        static async Task<Results<Ok<GenreDTO>, NotFound>> GetById(int id, IGenresRepository genresRepository)
        {
            var genre = await genresRepository.GetByIdAsync(id);

            if (genre is null)
            {
                return TypedResults.NotFound();
            }

            var genreDTO = new GenreDTO
            {
                Id = genre.Id,
                Name = genre.Name
            };

            return TypedResults.Ok(genreDTO);
        }

        static async Task<Created<GenreDTO>> Create(CreateGenreDTO createGenreDTO, IGenresRepository genresRepository,
            IOutputCacheStore outputCacheStore)
        {
            var genre = new Genre
            {
                Name = createGenreDTO.Name
            };

            var id = await genresRepository.CreateAsync(genre);
            await outputCacheStore.EvictByTagAsync("genres-get", default);
            
            var genreDTO = new GenreDTO
            {
                Id = genre.Id,
                Name = genre.Name
            };
            
            return TypedResults.Created($"/genres/{id}", genreDTO);
        }

        static async Task<Results<NotFound, NoContent>> Update(int id, CreateGenreDTO createGenreDTO,
            IGenresRepository repository, IOutputCacheStore outputCacheStore)
        {
            var exists = await repository.ExistsAsync(id);

            if (!exists)
                return TypedResults.NotFound();

            var genre = new Genre
            {
                Id = id,
                Name = createGenreDTO.Name
            };

            await repository.UpdateAsync(genre);
            await outputCacheStore.EvictByTagAsync("genres-get", default);
            return TypedResults.NoContent();
        }

        static async Task<Results<NotFound, NoContent>> Delete(int id, IGenresRepository genresRepository,
            IOutputCacheStore outputCacheStore)
        {
            var exists = await genresRepository.ExistsAsync(id);

            if (!exists)
                return TypedResults.NotFound();

            await genresRepository.DeleteAsync(id);
            await outputCacheStore.EvictByTagAsync("genres-get", default);
            return TypedResults.NoContent();
        }
    }
}
