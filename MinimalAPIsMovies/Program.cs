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
    var genres = await genresRepository.GetAllAsync();
    return genres;

}).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)));

app.MapGet("/genres/{id:int}", async (int id, IGenresRepository genresRepository) =>
{
    var genre = await genresRepository.GetById(id);

    if (genre is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(genre);
});

app.MapPost("/genres", async (Genre genre, IGenresRepository genresRepository) =>
{
    await genresRepository.CreateAsync(genre);
    return TypedResults.Created($"/genres/{genre.Id}", genre);
});

//Middlewares zone - END

app.Run();