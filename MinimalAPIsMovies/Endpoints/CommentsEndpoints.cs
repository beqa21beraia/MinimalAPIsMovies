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
            IMapper mapper, IUsersService usersService, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("CommentsEndpoints");
            logger.LogInformation("Creating a new comment for movieId: {MovieId}", movieId);

            if (!await moviesRepository.ExistsAsync(movieId))
            {
                logger.LogWarning("Movie with id: {MovieId} was not found", movieId);
                return TypedResults.NotFound();
            }

            var user = await usersService.GetUserAsync();

            if (user is null)
            {
                logger.LogWarning("User was not found while creating comment for movieId: {MovieId}", movieId);
                return TypedResults.BadRequest("user not found");
            }

            var comment = mapper.Map<Comment>(createCommentDTO);
            comment.MovieId = movieId;
            comment.UserId = user.Id;
            var id = await commentsRepository.CreateAsync(comment);
            await outputCacheStore.EvictByTagAsync("comments-get", default);
            var commentDTO = mapper.Map<CommentDTO>(comment);

            logger.LogInformation("Comment created successfully with id: {Id} for movieId: {MovieId}", id, movieId);
            return TypedResults.CreatedAtRoute(commentDTO, "GetCommentById", new { id, movieId });
        }

        static async Task<Results<Ok<List<CommentDTO>>, NotFound>> GetAllAsync(int movieId,
            ICommentsRepository commentsRepository, IMoviesRepository moviesRepository,
            IMapper mapper, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("CommentsEndpoints");
            logger.LogInformation("Fetching all comments for movieId: {MovieId}", movieId);

            if (!await moviesRepository.ExistsAsync(movieId))
            {
                logger.LogWarning("Movie with id: {MovieId} was not found", movieId);
                return TypedResults.NotFound();
            }

            var comments = await commentsRepository.GetAllAsync(movieId);
            var commentsDTO = mapper.Map<List<CommentDTO>>(comments);

            logger.LogInformation("Returning {Count} comments for movieId: {MovieId}", commentsDTO.Count, movieId);
            return TypedResults.Ok(commentsDTO);
        }

        static async Task<Results<Ok<CommentDTO>, NotFound>> GetByIdAsync(int movieId, int commentId,
            ICommentsRepository commentsRepository, IMoviesRepository moviesRepository,
            IMapper mapper, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("CommentsEndpoints");
            logger.LogInformation("Fetching comment with id: {CommentId} for movieId: {MovieId}", commentId, movieId);

            if (!await moviesRepository.ExistsAsync(movieId))
            {
                logger.LogWarning("Movie with id: {MovieId} was not found", movieId);
                return TypedResults.NotFound();
            }

            var comment = await commentsRepository.GetByIdAsync(commentId);

            if (comment is null)
            {
                logger.LogWarning("Comment with id: {CommentId} was not found", commentId);
                return TypedResults.NotFound();
            }

            var commentDTO = mapper.Map<CommentDTO>(comment);

            logger.LogInformation("Returning comment with id: {CommentId}", commentId);
            return TypedResults.Ok(commentDTO);
        }

        static async Task<Results<NoContent, NotFound, ForbidHttpResult>> UpdateAsync(int movieId, int id,
            CreateCommentDTO createCommentDTO, ICommentsRepository commentsRepository,
            IMoviesRepository moviesRepository, IOutputCacheStore outputCacheStore, IMapper mapper,
            IUsersService usersService, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("CommentsEndpoints");
            logger.LogInformation("Updating comment with id: {Id} for movieId: {MovieId}", id, movieId);

            if (!await moviesRepository.ExistsAsync(movieId))
            {
                logger.LogWarning("Movie with id: {MovieId} was not found", movieId);
                return TypedResults.NotFound();
            }

            var commentFromDB = await commentsRepository.GetByIdAsync(id);

            if (commentFromDB is null)
            {
                logger.LogWarning("Comment with id: {Id} was not found for update", id);
                return TypedResults.NotFound();
            }

            var user = await usersService.GetUserAsync();

            if (user is null)
            {
                logger.LogWarning("User was not found while updating comment with id: {Id}", id);
                return TypedResults.NotFound();
            }

            if (commentFromDB.UserId != user.Id)
            {
                logger.LogWarning("User {UserId} is not authorized to update comment with id: {Id}", user.Id, id);
                return TypedResults.Forbid();
            }

            commentFromDB.Body = createCommentDTO.Body;

            await commentsRepository.UpdateAsync(commentFromDB);
            await outputCacheStore.EvictByTagAsync("comments-get", default);

            logger.LogInformation("Comment with id: {Id} updated successfully", id);
            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound, ForbidHttpResult>> DeleteAsync(int movieId, int id,
            ICommentsRepository commentsRepository, IMoviesRepository moviesRepository,
            IOutputCacheStore outputCacheStore, IUsersService usersService, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("CommentsEndpoints");
            logger.LogInformation("Deleting comment with id: {Id} for movieId: {MovieId}", id, movieId);

            if (!await moviesRepository.ExistsAsync(movieId))
            {
                logger.LogWarning("Movie with id: {MovieId} was not found", movieId);
                return TypedResults.NotFound();
            }

            var commentFromDB = await commentsRepository.GetByIdAsync(id);

            if (commentFromDB is null)
            {
                logger.LogWarning("Comment with id: {Id} was not found for deletion", id);
                return TypedResults.NotFound();
            }

            var user = await usersService.GetUserAsync();

            if (user is null)
            {
                logger.LogWarning("User was not found while deleting comment with id: {Id}", id);
                return TypedResults.NotFound();
            }

            if (commentFromDB.UserId != user.Id)
            {
                logger.LogWarning("User {UserId} is not authorized to delete comment with id: {Id}", user.Id, id);
                return TypedResults.Forbid();
            }

            await commentsRepository.DeleteAsync(id);
            await outputCacheStore.EvictByTagAsync("comments-get", default);

            logger.LogInformation("Comment with id: {Id} deleted successfully", id);
            return TypedResults.NoContent();
        }
    }
}