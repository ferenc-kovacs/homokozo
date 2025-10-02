var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddStorage(builder.Configuration);

builder.Services.AddLogging();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/user/{id:int}", async (int id, IUserService userService) =>
{
    var userResult = await userService.GetUser(new GetUserInput
    {
        Id = id
    });
    if (userResult.IsFailure)
    {
        // todo handle error
        throw new Exception();
    }
    return userResult.Value;
});

app.MapPost("/user", async (CreateUserInput input, IUserService userService) =>
{
    var createUserResult = await userService.CreateUser(input);
    if (createUserResult.IsFailure)
    {
        throw new Exception();
    }
    return createUserResult.Value;
});

app.Run();
