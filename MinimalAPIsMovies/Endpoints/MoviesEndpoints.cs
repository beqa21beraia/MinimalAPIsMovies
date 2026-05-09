using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Identity.Client;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Filters;
using MinimalAPIsMovies.Interfaces;
using System.Security.Cryptography;

namespace MinimalAPIsMovies.Endpoints
{
    public static class MoviesEndpoints
    {
        private readonly static string _container = "movies";
        public static RouteGroupBuilder MapMovies(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllAsync)
                .CacheOutput(c => c.Expire(TimeSpan.FromMinutes(1)).Tag("movies-get"));
            group.MapGet("/{id:int}", GetByIdAsync);

            group.MapPost("/", CreateAsync)
                .DisableAntiforgery()
                .AddEndpointFilter<ValidationFilter<CreateMovieDTO>>()
                .RequireAuthorization("isadmin");

            group.MapPut("/{id:int}", UpdateAsync)
                .DisableAntiforgery()
                .AddEndpointFilter<ValidationFilter<CreateMovieDTO>>()
                .RequireAuthorization("isadmin");

            group.MapDelete("/{id:int}", DeleteAsync).RequireAuthorization("isadmin");
            group.MapPost("/{id:int}/assignGenres", AssignGenresAsync).RequireAuthorization("isadmin");
            group.MapPost("/{id:int}/assignActors", AssignActorsAsync).RequireAuthorization("isadmin");
            return group;
        }

        static async Task<Created<MovieDTO>> CreateAsync([FromForm] CreateMovieDTO createMovieDTO,
            IMoviesRepository moviesRepository, IOutputCacheStore outputCacheStore,
            IMapper mapper, IFileStorage fileStorage, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("MoviesEndpoints");
            logger.LogInformation("Creating a new movie with title: {Title}", createMovieDTO.Title);

            var movie = mapper.Map<Movie>(createMovieDTO);

            if (createMovieDTO.Poster is not null)
            {
                var url = await fileStorage.StoreAsync(_container, createMovieDTO.Poster);
                movie.Poster = url;
                logger.LogInformation("Poster stored at: {Url}", url);
            }

            var id = await moviesRepository.CreateAsync(movie);
            await outputCacheStore.EvictByTagAsync("movies-get", default);
            var movieDTO = mapper.Map<MovieDTO>(movie);

            logger.LogInformation("Movie created successfully with id: {Id}", id);
            return TypedResults.Created($"movies/{id}", movieDTO);
        }

        static async Task<Ok<List<MovieDTO>>> GetAllAsync(IMoviesRepository moviesRepository,
            IMapper mapper, ILoggerFactory loggerFactory, int page = 1, int recordsPerPage = 10)
        {
            var logger = loggerFactory.CreateLogger("MoviesEndpoints");
            logger.LogInformation("Fetching all movies - Page: {Page}, RecordsPerPage: {RecordsPerPage}",
                page, recordsPerPage);

            var pagination = new PaginationDTO { Page = page, RecordsPerPage = recordsPerPage };
            var movies = await moviesRepository.GetAllAsync(pagination);
            var moviesDTO = mapper.Map<List<MovieDTO>>(movies);

            logger.LogInformation("Returning {Count} movies", moviesDTO.Count);
            return TypedResults.Ok(moviesDTO);
        }

        static async Task<Results<Ok<MovieDTO>, NotFound>> GetByIdAsync(int id,
            IMoviesRepository moviesRepository, IMapper mapper, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("MoviesEndpoints");
            logger.LogInformation("Fetching movie with id: {Id}", id);

            var movie = await moviesRepository.GetByIdAsync(id);

            if (movie is null)
            {
                logger.LogWarning("Movie with id: {Id} was not found", id);
                return TypedResults.NotFound();
            }

            var movieDTO = mapper.Map<MovieDTO>(movie);

            logger.LogInformation("Returning movie with id: {Id}", id);
            return TypedResults.Ok(movieDTO);
        }

        static async Task<Results<NoContent, NotFound>> UpdateAsync(int id,
            [FromForm] CreateMovieDTO createMovieDTO, IMoviesRepository moviesRepository,
            IFileStorage fileStorage, IOutputCacheStore outputCacheStore,
            IMapper mapper, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("MoviesEndpoints");
            logger.LogInformation("Updating movie with id: {Id}", id);

            var movieDB = await moviesRepository.GetByIdAsync(id);

            if (movieDB is null)
            {
                logger.LogWarning("Movie with id: {Id} was not found for update", id);
                return TypedResults.NotFound();
            }

            var movieToUpdate = mapper.Map<Movie>(createMovieDTO);
            movieToUpdate.Id = id;
            movieToUpdate.Poster = movieDB.Poster;

            if (createMovieDTO.Poster is not null)
            {
                var url = await fileStorage.EditAsync(movieToUpdate.Poster,
                    _container, createMovieDTO.Poster);
                movieToUpdate.Poster = url;
                logger.LogInformation("Poster updated at: {Url}", url);
            }

            await moviesRepository.UpdateAsync(movieToUpdate);
            await outputCacheStore.EvictByTagAsync("movies-get", default);

            logger.LogInformation("Movie with id: {Id} updated successfully", id);
            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound>> DeleteAsync(int id,
            IMoviesRepository moviesRepository, IOutputCacheStore outputCacheStore,
            IFileStorage fileStorage, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("MoviesEndpoints");
            logger.LogInformation("Deleting movie with id: {Id}", id);

            var movieDB = await moviesRepository.GetByIdAsync(id);

            if (movieDB is null)
            {
                logger.LogWarning("Movie with id: {Id} was not found for deletion", id);
                return TypedResults.NotFound();
            }

            if (movieDB.Poster is not null)
            {
                await fileStorage.DeleteAsync(movieDB.Poster, _container);
                logger.LogInformation("Poster deleted for movie with id: {Id}", id);
            }

            await moviesRepository.DeleteAsync(id);
            await outputCacheStore.EvictByTagAsync("movies-get", default);

            logger.LogInformation("Movie with id: {Id} deleted successfully", id);
            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound, BadRequest<string>>> AssignGenresAsync(
            int id, List<int> genresIds, IMoviesRepository moviesRepository,
            IGenresRepository genresRepository, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("MoviesEndpoints");
            logger.LogInformation("Assigning genres to movie with id: {Id}", id);

            if (!await moviesRepository.ExistsAsync(id))
            {
                logger.LogWarning("Movie with id: {Id} was not found for genre assignment", id);
                return TypedResults.NotFound();
            }

            var existingGenreIds = new List<int>();

            if (genresIds.Count != 0)
            {
                existingGenreIds = await genresRepository.ExistsAsync(genresIds);
            }

            if (existingGenreIds.Count != genresIds.Count)
            {
                var nonExistingGenreIds = genresIds.Except(existingGenreIds);
                var nonExistingGenreIdsCSV = string.Join(",", nonExistingGenreIds);
                logger.LogWarning("Genres with ids: {Ids} do not exist", nonExistingGenreIdsCSV);
                return TypedResults
                    .BadRequest($"The genres of Id {nonExistingGenreIdsCSV} does not exists");
            }

            await moviesRepository.AssignAsync(id, genresIds);

            logger.LogInformation("Genres assigned successfully to movie with id: {Id}", id);
            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound, BadRequest<string>>> AssignActorsAsync(
            int id, List<AssignActorMovieDTO> actorsDTO, IMoviesRepository moviesRepository,
            IActorsRepository actorsRepository, IMapper mapper, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("MoviesEndpoints");
            logger.LogInformation("Assigning actors to movie with id: {Id}", id);

            if (!await moviesRepository.ExistsAsync(id))
            {
                logger.LogWarning("Movie with id: {Id} was not found for actor assignment", id);
                return TypedResults.NotFound();
            }

            var existingActors = new List<int>();
            var actorsIDs = actorsDTO.Select(a => a.ActorId).ToList();

            if (actorsDTO.Count != 0)
            {
                existingActors = await actorsRepository.ExistsAsync(actorsIDs);
            }

            if (existingActors.Count != actorsDTO.Count)
            {
                var nonExistingActors = actorsIDs.Except(existingActors);
                var nonExistingActorsCSV = string.Join(",", nonExistingActors);
                logger.LogWarning("Actors with ids: {Ids} do not exist", nonExistingActorsCSV);
                return TypedResults
                    .BadRequest($"The actors with Id {nonExistingActorsCSV} does not exists");
            }

            var actors = mapper.Map<List<ActorMovie>>(actorsDTO);
            await moviesRepository.AssignAsync(id, actors);

            logger.LogInformation("Actors assigned successfully to movie with id: {Id}", id);
            return TypedResults.NoContent();
        }
    }
}