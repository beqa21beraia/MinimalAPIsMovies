using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Interfaces;

namespace MinimalAPIsMovies.Endpoints
{
    public static class CommentsEndpoints
    {
        public static RouteGroupBuilder MapComments(this RouteGroupBuilder group)
        {
            group.MapPost("/", CreateAsync);
            group.MapGet("/", GetAllAsync)
                .CacheOutput(c => c.Expire(TimeSpan.FromMinutes(1)).Tag("comments-get"));
            group.MapGet("/{id:int}", GetByIdAsync).WithName("GetCommentById");
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

        static async Task<Results<Ok<CommentDTO>, NotFound>> GetByIdAsync(int movieId, int id,
            ICommentsRepository commentsRepository, IMoviesRepository moviesRepository,
            IMapper mapper)
        {
            if (!await moviesRepository.ExistsAsync(movieId))
            {
                return TypedResults.NotFound();
            }

            var comment = await commentsRepository.GetByIdAsync(id);

            if (comment is null)
            {
                return TypedResults.NotFound();
            }

            var commentDTO = mapper.Map<CommentDTO>(comment);

            return TypedResults.Ok(commentDTO);
        } 
    }
}
