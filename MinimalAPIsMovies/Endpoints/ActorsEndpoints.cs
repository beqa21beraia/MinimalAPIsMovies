using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Interfaces;
using MinimalAPIsMovies.Services;

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
            group.MapPost("/", CreateAsync).DisableAntiforgery();
            return group;
        }

        static async Task<Ok<List<ActorDTO>>> GetAllAsync(IActorsRepository repository,
            IMapper mapper)
        {
            var actors = await repository.GetAllAsync();
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


    }
}
