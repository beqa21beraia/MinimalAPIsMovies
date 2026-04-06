using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Filters;
using MinimalAPIsMovies.Interfaces;
using System.ClientModel.Primitives;

namespace MinimalAPIsMovies.Endpoints
{
    public static class CommentsEndpoints
    {
        public static RouteGroupBuilder MapComments(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllAsync)
                .CacheOutput(c => c.Expire(TimeSpan.FromMinutes(1)).Tag("comments-get"));
            group.MapGet("/{commentId:int}", GetByIdAsync).WithName("GetCommentById");
            group.MapPost("/", CreateAsync).AddEndpointFilter<ValidationFilter<CreateCommentDTO>>();
            group.MapPut("/{commentId:int}", UpdateAsync).AddEndpointFilter<ValidationFilter<CreateCommentDTO>>();
            group.MapDelete("/{commentId:int}", DeleteAsync);
            return group;
        }

        static async Task<Results<CreatedAtRoute<CommentDTO>, NotFound>> CreateAsync(int movieId,
            CreateCommentDTO createCommentDTO, ICommentsRepository commentsRepository, 
            IMoviesRepository moviesRepository, IOutputCacheStore outputCacheStore, 
            IMapper mapper)
        {
            if (! await moviesRepository.ExistsAsync(movieId))
            {
                return TypedResults.NotFound();
            }

            var comment = mapper.Map<Comment>(createCommentDTO);
            comment.MovieId = movieId;
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

        static async Task<Results<NoContent, NotFound>> UpdateAsync(int movieId, int commentId,
            CreateCommentDTO createCommentDTO, ICommentsRepository commentsRepository, 
            IMoviesRepository moviesRepository, IOutputCacheStore outputCacheStore, IMapper mapper)
        {
            if (!await moviesRepository.ExistsAsync(movieId))
            {
                return TypedResults.NotFound();
            }

            if (!await commentsRepository.ExistsAsync(commentId))
            {
                return TypedResults.NotFound();
            }
            
            var comment = mapper.Map<Comment>(createCommentDTO);
            comment.Id = commentId;
            comment.MovieId = movieId;
            
            await commentsRepository.UpdateAsync(comment);
            await outputCacheStore.EvictByTagAsync("comments-get", default);
            
            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound>> DeleteAsync(int movieId, int commentId,
            ICommentsRepository commentsRepository, IMoviesRepository moviesRepository,
            IOutputCacheStore outputCacheStore)
        {
            if (!await moviesRepository.ExistsAsync(movieId))
            {
                return TypedResults.NotFound();
            }

            if (!await commentsRepository.ExistsAsync(commentId))
            {
                return TypedResults.NotFound();
            }

            await commentsRepository.DeleteAsync(commentId);
            await outputCacheStore.EvictByTagAsync("comments-get", default);
                
            return TypedResults.NoContent();
        }
    }
}