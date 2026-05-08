using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Filters;
using MinimalAPIsMovies.Interfaces;
using System.Runtime.InteropServices;

namespace MinimalAPIsMovies.Endpoints
{
    public static class ActorsEndpoints
    {
        private readonly static string _container = "actors";
        public static RouteGroupBuilder MapActors(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllAsync)
                .CacheOutput(c => c.Expire(TimeSpan.FromMinutes(1)).Tag("actors-get"));
            group.MapGet("/{id:int}", GetByIdAsync);
            group.MapGet("getByName/{name}", GetByNameAsync);

            group.MapPost("/", CreateAsync)
                .DisableAntiforgery()
                .AddEndpointFilter<ValidationFilter<CreateActorDTO>>()
                .RequireAuthorization("isadmin");

            group.MapPut("/{id:int}", UpdateAsync)
                .DisableAntiforgery()
                .AddEndpointFilter<ValidationFilter<CreateActorDTO>>()
                .RequireAuthorization("isadmin");

            group.MapDelete("/{id:int}", DeleteAsync).RequireAuthorization("isadmin");
            return group;
        }

        static async Task<Ok<List<ActorDTO>>> GetAllAsync(IActorsRepository repository,
            IMapper mapper, ILoggerFactory loggerFactory, int page = 1, int recordsPerPage = 10)
        {
            var logger = loggerFactory.CreateLogger("ActorsEndpoints");
            logger.LogInformation("Fetching all actors - Page: {Page}, RecordsPerPage: {RecordsPerPage}",
                page, recordsPerPage);

            var pagination = new PaginationDTO { Page = page, recordsPerPage = recordsPerPage };
            var actors = await repository.GetAllAsync(pagination);
            var actorsDTO = mapper.Map<List<ActorDTO>>(actors);

            logger.LogInformation("Returning {Count} actors", actorsDTO.Count);
            return TypedResults.Ok(actorsDTO);
        }

        static async Task<Results<Ok<ActorDTO>, NotFound>> GetByIdAsync(int id,
            IActorsRepository repository,
            IMapper mapper, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("ActorsEndpoints");
            logger.LogInformation("Fetching actor with id: {Id}", id);

            var actor = await repository.GetByIdAsync(id);

            if (actor is null)
            {
                logger.LogWarning("Actor with id: {Id} was not found", id);
                return TypedResults.NotFound();
            }

            var actorDTO = mapper.Map<ActorDTO>(actor);

            logger.LogInformation("Returning actor with id: {Id}", id);
            return TypedResults.Ok(actorDTO);
        }

        static async Task<Ok<List<ActorDTO>>> GetByNameAsync(string name,
            IActorsRepository actorsRepository, IMapper mapper, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("ActorsEndpoints");
            logger.LogInformation("Fetching actors with name: {Name}", name);

            var actors = await actorsRepository.GetByNameAsync(name);
            var actorsDTO = mapper.Map<List<ActorDTO>>(actors);

            logger.LogInformation("Returning {Count} actors with name: {Name}", actorsDTO.Count, name);
            return TypedResults.Ok(actorsDTO);
        }

        static async Task<Created<ActorDTO>> CreateAsync(
            [FromForm] CreateActorDTO createActorDTO, IActorsRepository actorsRepository,
            IOutputCacheStore outputCacheStore, IMapper mapper,
            IFileStorage fileStorage, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("ActorsEndpoints");
            logger.LogInformation("Creating a new actor with name: {Name}", createActorDTO.Name);

            var actor = mapper.Map<Actor>(createActorDTO);

            if (createActorDTO.Picture is not null)
            {
                var url = await fileStorage.StoreAsync(_container, createActorDTO.Picture);
                actor.Picture = url;
                logger.LogInformation("Picture stored at: {Url}", url);
            }

            var id = await actorsRepository.CreateAsync(actor);
            await outputCacheStore.EvictByTagAsync("actors-get", default);
            var actorDTO = mapper.Map<ActorDTO>(actor);

            logger.LogInformation("Actor created successfully with id: {Id}", id);
            return TypedResults.Created($"actors/{id}", actorDTO);
        }

        static async Task<Results<NoContent, NotFound>> UpdateAsync(int id,
            [FromForm] CreateActorDTO createActorDTO, IActorsRepository actorsRepository,
            IFileStorage fileStorage, IOutputCacheStore outputCacheStore,
            IMapper mapper, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("ActorsEndpoints");
            logger.LogInformation("Updating actor with id: {Id}", id);

            var actorDB = await actorsRepository.GetByIdAsync(id);

            if (actorDB is null)
            {
                logger.LogWarning("Actor with id: {Id} was not found for update", id);
                return TypedResults.NotFound();
            }

            var actorToUpdate = mapper.Map<Actor>(createActorDTO);
            actorToUpdate.Id = id;
            actorToUpdate.Picture = actorDB.Picture;

            if (createActorDTO.Picture is not null)
            {
                var url = await fileStorage.EditAsync(actorToUpdate.Picture,
                    _container, createActorDTO.Picture);
                actorToUpdate.Picture = url;
                logger.LogInformation("Picture updated at: {Url}", url);
            }

            await actorsRepository.UpdateAsync(actorToUpdate);
            await outputCacheStore.EvictByTagAsync("actors-get", default);

            logger.LogInformation("Actor with id: {Id} updated successfully", id);
            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound>> DeleteAsync(int id,
            IActorsRepository actorsRepository, IOutputCacheStore outputCacheStore,
            IFileStorage fileStorage, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("ActorsEndpoints");
            logger.LogInformation("Deleting actor with id: {Id}", id);

            var actorDB = await actorsRepository.GetByIdAsync(id);

            if (actorDB is null)
            {
                logger.LogWarning("Actor with id: {Id} was not found for deletion", id);
                return TypedResults.NotFound();
            }

            if (actorDB.Picture is not null)
            {
                await fileStorage.DeleteAsync(actorDB.Picture, _container);
                logger.LogInformation("Picture deleted for actor with id: {Id}", id);
            }

            await actorsRepository.DeleteAsync(id);
            await outputCacheStore.EvictByTagAsync("actors-get", default);

            logger.LogInformation("Actor with id: {Id} deleted successfully", id);
            return TypedResults.NoContent();
        }
    }
}