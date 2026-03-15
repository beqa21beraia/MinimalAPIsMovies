using Microsoft.AspNetCore.OutputCaching;
using MinimalAPIsMovies.Entities;
using MinimalAPIsMovies.Interfaces;
using MinimalAPIsMovies.Repositories;

var builder = WebApplication.CreateBuilder(args);

//Services zone - BEGIN

builder.Services.AddScoped<IGenresRepository, GenresRepository>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(configuration =>
    {
        configuration.WithOrigins(builder.Configuration["allowedOrigins"]!)
        .AllowAnyMethod()
        .AllowAnyHeader();
    });

    options.AddPolicy("AllowAll", configuration =>
    {
        configuration.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});
builder.Services.AddOutputCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Services zone - END

var app = builder.Build();

//Middlewares zone - BEGIN

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseOutputCache();

app.MapGet("/genres", async (IGenresRepository genresRepository) =>
{
    return await genresRepository.GetAllAsync();

}).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("genres-get"));

app.MapGet("/genres/{id:int}", async (int id, IGenresRepository genresRepository) =>
{
    var genre = await genresRepository.GetByIdAsync(id);

    if (genre is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(genre);
});

app.MapPost("/genres", async (Genre genre, IGenresRepository genresRepository,
    IOutputCacheStore outputCacheStore) =>
{
    await genresRepository.CreateAsync(genre);
    await outputCacheStore.EvictByTagAsync("genres-get", default);
    return TypedResults.Created($"/genres/{genre.Id}", genre);
});

app.MapPut("/genres/{id:int}", async (int id, Genre genre,
    IGenresRepository repository, IOutputCacheStore outputCacheStore) =>
{
    var exists = await repository.ExistsAsync(id);

    if(!exists)
        return Results.NotFound();

    await repository.UpdateAsync(genre);
    await outputCacheStore.EvictByTagAsync("genres-get", default);
    return Results.NoContent();
});

app.MapDelete("/genres/{id:int}", async (int id, IGenresRepository genresRepository,
    IOutputCacheStore outputCacheStore) =>
{
    var exists = await genresRepository.ExistsAsync(id);

    if (!exists)
        return Results.NotFound();

    await genresRepository.DeleteAsync(id);
    await outputCacheStore.EvictByTagAsync("genres-get", default);
    return Results.NoContent();
});

//Middlewares zone - END

app.Run();