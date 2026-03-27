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
            return group;
        }

        static async Task<Results<Created<CommentDTO>, NotFound>> CreateAsync(int movieId,
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

            return TypedResults.Created($"/comment/{id}", commentDTO);
        }
    }
}
