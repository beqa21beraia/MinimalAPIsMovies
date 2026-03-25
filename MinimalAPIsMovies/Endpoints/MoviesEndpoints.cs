using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Interfaces;
using MinimalAPIsMovies.Services;
using System.Security.Cryptography;

namespace MinimalAPIsMovies.Endpoints
{
    public static class MoviesEndpoints
    {
        private readonly static string _container = "movies";
        public static RouteGroupBuilder MapMovies(this RouteGroupBuilder group)
        {
            group.MapPost("/", CreateAsync).DisableAntiforgery();
            group.MapGet("/", GetAllAsync)
                .CacheOutput(c => c.Expire(TimeSpan.FromMinutes(1)).Tag("movies-get"));
            group.MapGet("/{id:int}", GetByIdAsync);
            return group;
        }

        static async Task<Created<MovieDTO>> CreateAsync([FromForm] CreateMovieDTO createMovieDTO,
            IMoviesRepository moviesRepository, IOutputCacheStore outputCacheStore,
            IMapper mapper, IFileStorage fileStorage)
        {
            var movie = mapper.Map<Movie>(createMovieDTO);

            if (createMovieDTO.Poster is not null)
            {
                var url = await fileStorage.StoreAsync(_container, createMovieDTO.Poster);
                movie.Poster = url;
            }

            var id = await moviesRepository.CreateAsync(movie);
            await outputCacheStore.EvictByTagAsync("movies-get", default);
            var movieDTO = mapper.Map<MovieDTO>(movie);

            return TypedResults.Created($"movies/{id}", movieDTO);
        }

        static async Task<Ok<List<MovieDTO>>> GetAllAsync(IMoviesRepository moviesRepository,
            IMapper mapper, int page = 1, int recordsPerPage = 10)
        {
            var pagination = new PaginationDTO { Page = page, RecordsPerPage = recordsPerPage };
            var movies = await moviesRepository.GetAllAsync(pagination);
            var moviesDTO = mapper.Map<List<MovieDTO>>(movies);
            return TypedResults.Ok(moviesDTO);
        }

        static async Task<Results<Ok<MovieDTO>, NotFound>> GetByIdAsync(int id, 
            IMoviesRepository moviesRepository, IMapper mapper)
        {
            var movie = await moviesRepository.GetByIdAsync(id);
                
            if (movie is null)
            {
                return TypedResults.NotFound();
            }

            var movieDTO = mapper.Map<MovieDTO>(movie);
            return TypedResults.Ok(movieDTO);
        }
    }
}
