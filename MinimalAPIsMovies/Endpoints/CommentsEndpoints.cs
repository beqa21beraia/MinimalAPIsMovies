using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Filters;
using MinimalAPIsMovies.Interfaces;
using MinimalAPIsMovies.Services;
using System.ClientModel.Primitives;

namespace MinimalAPIsMovies.Endpoints
{
    public static class CommentsEndpoints
    {
        public static RouteGroupBuilder MapComments(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllAsync)
                .CacheOutput(c => c.Expire(TimeSpan.FromMinutes(1)).Tag("comments-get"));
            group.MapGet("/{id:int}", GetByIdAsync).WithName("GetCommentById");
            group.MapPost("/", CreateAsync)
                .AddEndpointFilter<ValidationFilter<CreateCommentDTO>>()
                .RequireAuthorization();
            group.MapPut("/{id:int}", UpdateAsync)
                .AddEndpointFilter<ValidationFilter<CreateCommentDTO>>()
                .RequireAuthorization();
            group.MapDelete("/{id:int}", DeleteAsync).RequireAuthorization();
            return group;
        }

        static async Task<Results<CreatedAtRoute<CommentDTO>, NotFound, BadRequest<string>>> CreateAsync(int movieId,
            CreateCommentDTO createCommentDTO, ICommentsRepository commentsRepository, 
            IMoviesRepository moviesRepository, IOutputCacheStore outputCacheStore, 
            IMapper mapper, IUsersService usersService)
        {
            if (! await moviesRepository.ExistsAsync(movieId))
            {
                return TypedResults.NotFound();
            }

            var user = await usersService.GetUserAsync();

            if (user is null)
            {
                return TypedResults.BadRequest("user not found");
            }

            var comment = mapper.Map<Comment>(createCommentDTO);
            comment.MovieId = movieId;
            comment.UserId = user.Id;
            var id = await commentsRepository.CreateAsync(comment);
            await outputCacheStore.EvictByTagAsync("comments-get", default);
            var commentDTO = mapper.Map<CommentDTO>(comment);

            return TypedResults.CreatedAtRoute(commentDTO, "GetCommentById", new { id, movieId });
        }

        static async Task<Results<Ok<List<CommentDTO>>, NotFound>> GetAllAsync(int movieId,
            ICommentsRepository commentsRepository, IMoviesRepository moviesRepository,
            IMapper mapper)
        {
            if (!await moviesRepository.ExistsAsync(movieId))
            {
                return TypedResults.NotFound();
            }

            var comments = await commentsRepository.GetAllAsync(movieId);
            var commentsDTO = mapper.Map<List<CommentDTO>>(comments);

            return TypedResults.Ok(commentsDTO);
        }
        
        static async Task<Results<Ok<CommentDTO>, NotFound>> GetByIdAsync(int movieId, int commentId,
            ICommentsRepository commentsRepository, IMoviesRepository moviesRepository,
            IMapper mapper)
        {
            if (!await moviesRepository.ExistsAsync(movieId))
            {
                return TypedResults.NotFound();
            }

            var comment = await commentsRepository.GetByIdAsync(commentId);

            if (comment is null)
            {
                return TypedResults.NotFound();
            }

            var commentDTO = mapper.Map<CommentDTO>(comment);

            return TypedResults.Ok(commentDTO);
        } 

        static async Task<Results<NoContent, NotFound, ForbidHttpResult>> UpdateAsync(int movieId, int id,
            CreateCommentDTO createCommentDTO, ICommentsRepository commentsRepository, 
            IMoviesRepository moviesRepository, IOutputCacheStore outputCacheStore, IMapper mapper,
            IUsersService usersService)
        {
            if (!await moviesRepository.ExistsAsync(movieId))
            {
                return TypedResults.NotFound();
            }

            var commentFromDB = await commentsRepository.GetByIdAsync(id);

            if (commentFromDB is null)
            {
                return TypedResults.NotFound();
            }

            var user = await usersService.GetUserAsync();

            if (user is null)
            {
                return TypedResults.NotFound();
            }

            if (commentFromDB.UserId != user.Id)
            {
                return TypedResults.Forbid();
            }
            
            commentFromDB.Body = createCommentDTO.Body;
            
            await commentsRepository.UpdateAsync(commentFromDB);
            await outputCacheStore.EvictByTagAsync("comments-get", default);
            
            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound, ForbidHttpResult>> DeleteAsync(int movieId, int id,
            ICommentsRepository commentsRepository, IMoviesRepository moviesRepository,
            IOutputCacheStore outputCacheStore, IUsersService usersService)
        {
            if (!await moviesRepository.ExistsAsync(movieId))
            {
                return TypedResults.NotFound();
            }

            var commentFromDB = await commentsRepository.GetByIdAsync(id);

            if (commentFromDB is null)
            {
                return TypedResults.NotFound();
            }

            var user = await usersService.GetUserAsync();

            if (user is null)
            {
                return TypedResults.NotFound();
            }

            if (commentFromDB.UserId != user.Id)
            {
                return TypedResults.Forbid();
            }

            await commentsRepository.DeleteAsync(id);
            await outputCacheStore.EvictByTagAsync("comments-get", default);
                
            return TypedResults.NoContent();
        }
    }
}