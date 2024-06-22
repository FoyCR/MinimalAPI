using Microsoft.AspNetCore.Mvc;

#region Configuring

var builder = WebApplication.CreateBuilder(args);

//configuring CORS
builder.Services.AddCors(op => op.AddDefaultPolicy(
    builder =>
    {
        builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
    }));

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMvc(x => x.EnableEndpointRouting = false);
builder.Services.AddSingleton<ICache, SmallCache>();
builder.Services.AddKeyedSingleton<ICache, BigCache>("big");

var app = builder.Build();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseMvc();

#endregion Configuring

#region Endpoints

app.MapGet("/", () => "My minimal API is up and running!");

app.MapGet("/users", IResult ([FromQuery] int id) =>
{
    return Results.Ok(new { message = $"You send the id {id}" });
});

app.MapPost("/users", IResult ([FromQuery] int id, [FromQuery] string name) =>
{
    return Results.Ok(new { message = $"You send the id {id} with the name {name}" });
});

app.MapPost("/users2", IResult ([FromForm] int id, [FromForm] string name) =>
{
    return Results.Ok(new { message = $"You send the id {id} with the name {name}" });
}).DisableAntiforgery();

app.MapPost("/users3", IResult ([FromForm] User user) =>
{
    return Results.Ok(new { message = $"You send a model with the id {user.id} and name {user.name} " });
}).DisableAntiforgery();

app.MapPost("/users4", IResult ([FromBody] UserId userId) =>
{
    return Results.Ok(new { message = $"You send the id {userId.id}" });
});

app.MapPost("/users5", IResult ([FromBody] User user) =>
{
    return Results.Ok(new { message = $"You send a model with the id {user.id} and name {user.name} " });
});

app.MapPost("/users6", IResult ([FromHeader] int id) =>
{
    return Results.Ok(new { message = $"You send the id {id} in the header" });
});

app.MapGet("/users7", IResult ([FromQuery] int id, [FromServices] ICache cacheService) =>
{
    string cacheMessage = (string)cacheService.Get($"MyKey{id}");
    return Results.Ok(new { message = $"You send the id {id}.", cache = cacheMessage });
});

app.MapGet("/users8", IResult ([FromQuery] int id, [FromKeyedServices("big")] ICache cacheService) =>
{
    string cacheMessage = (string)cacheService.Get($"MyKey{id}");
    return Results.Ok(new { message = $"You send the id {id}.", cache = cacheMessage });
});

#endregion Endpoints

app.Run();

#region Helpers/Utilities

record User
{
    public int id { get; set; }
    public string name { get; set; }
}

internal class UserId
{
    public int id { get; set; }
}

public interface ICache
{
    object Get(string key);
}

public class SmallCache : ICache
{
    public object Get(string key) => $"Resolving {key} from small cache.";
}

public class BigCache : ICache
{
    public object Get(string key) => $"Resolving {key} from big cache.";
}

#endregion Helpers/Utilities