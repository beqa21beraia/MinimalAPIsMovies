using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Interfaces;
using MinimalAPIsMovies.Services;
using System.Runtime.InteropServices;

namespace MinimalAPIsMovies.Endpoints
{
    public static class ActorsEndpoints
    {
        private readonly static string container = "actors";
        public static RouteGroupBuilder MapActors(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllAsync)
                .CacheOutput(c => c.Expire(TimeSpan.FromMinutes(1)).Tag("actors-get"));
            group.MapGet("/{id:int}", GetByIdAsync);
            group.MapGet("getByName/{name}", GetByNameAsync);
            group.MapPost("/", CreateAsync).DisableAntiforgery();
            group.MapPut("/{id:int}", UpdateAsync).DisableAntiforgery();
            group.MapDelete("/{id:int}", DeleteAsync);
            return group;
        }
        
        static async Task<Ok<List<ActorDTO>>> GetAllAsync(IActorsRepository repository,
            IMapper mapper, int page = 1, int recordsPerPage = 10)
        {
            var pagination = new PaginationDTO { Page = page, recordsPerPage = recordsPerPage };
            var actors = await repository.GetAllAsync(pagination);
            var actorsDTO = mapper.Map<List<ActorDTO>>(actors);
            return TypedResults.Ok(actorsDTO);
        }

        static async Task<Results<Ok<ActorDTO>, NotFound>> GetByIdAsync(int id,
            IActorsRepository repository,
            IMapper mapper)
        {
            var actor = await repository.GetByIdAsync(id);

            if (actor is null)
            {
                return TypedResults.NotFound();
            }

            var actorDTO = mapper.Map<ActorDTO>(actor);
            return TypedResults.Ok(actorDTO);
        }

        static async Task<Ok<List<ActorDTO>>> GetByNameAsync(string name,
            IActorsRepository actorsRepository, IMapper mapper)
        {
            var actors = await actorsRepository.GetByNameAsync(name);
            var actorsDTO = mapper.Map<List<ActorDTO>>(actors);
            return TypedResults.Ok(actorsDTO);
        }

        static async Task<Created<ActorDTO>> CreateAsync([FromForm] CreateActorDTO createActorDTO,
            IActorsRepository actorsRepository, IOutputCacheStore outputCacheStore,
            IMapper mapper, IFileStorage fileStorage)
        {
            var actor = mapper.Map<Actor>(createActorDTO);

            if (createActorDTO.Picture is not null)
            {
                var url = await fileStorage.StoreAsync(container, createActorDTO.Picture);
                actor.Picture = url;
            }

            var id = await actorsRepository.CreateAsync(actor);
            await outputCacheStore.EvictByTagAsync("actors-get", default);

            var actorDTO = mapper.Map<ActorDTO>(actor);

            return TypedResults.Created($"actors/{id}", actorDTO);
        }

        static async Task<Results<NoContent, NotFound>> UpdateAsync(int id,
            [FromForm] CreateActorDTO createActorDTO, IActorsRepository actorsRepository,
            IFileStorage fileStorage, IOutputCacheStore outputCacheStore,
            IMapper mapper)
        {
            var actorDB = await actorsRepository.GetByIdAsync(id);

            if (actorDB is null)
            {
                return TypedResults.NotFound();
            }

            var actorToUpdate = mapper.Map<Actor>(createActorDTO);
            actorToUpdate.Id = id;
            actorToUpdate.Picture = actorDB.Picture;

            if (createActorDTO.Picture is not null)
            {
                var url = await fileStorage.EditAsync(actorToUpdate.Picture,
                    container, createActorDTO.Picture);
                actorToUpdate.Picture = url;
            }

            await actorsRepository.UpdateAsync(actorToUpdate);
            await outputCacheStore.EvictByTagAsync("actors-get", default);

            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound>> DeleteAsync(int id,
            IActorsRepository actorsRepository, IOutputCacheStore outputCacheStore,
            IFileStorage fileStorage)
        {
            var actorDB = await actorsRepository.GetByIdAsync(id);

            if (actorDB is null)
            {
                return TypedResults.NotFound();
            }

            if (actorDB.Picture is not null)
            {
                await fileStorage.DeleteAsync(actorDB.Picture, container);
            }

            await actorsRepository.DeleteAsync(id);
            await outputCacheStore.EvictByTagAsync("actors-get", default);
            return TypedResults.NoContent();
        }
    }
}
