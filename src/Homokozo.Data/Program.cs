using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddStorage(builder.Configuration);

builder.Services.AddLogging();


const string SlidingPolicy = "SlidingPolicy";

// TODO fix ratelimiting
// https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit-samples?view=aspnetcore-9.0
builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString();
        }

        context.HttpContext.RequestServices
            .GetService<ILoggerFactory>()?
            .CreateLogger("Microsoft.AspNetCore.RateLimitingMiddleware")
            .LogWarning("OnRejected: {endpoint}", $"endpoint: {context.HttpContext.Request.Path} {context.HttpContext.Connection.RemoteIpAddress}");

        return new ValueTask();
    };

    options.AddPolicy(SlidingPolicy, context =>
    {
        var userName = "anonymous user";
        if (context.User.Identity is {  IsAuthenticated: true })
        {
            userName = context.User.Identity.Name;
        }
        return RateLimitPartition.GetSlidingWindowLimiter(userName, _ =>
        new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 10,
            QueueLimit = 1,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            SegmentsPerWindow = 4,
            Window = TimeSpan.FromSeconds(60),
        });
    });
});


var app = builder.Build();

app.UseRateLimiter();

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
}).RequireRateLimiting(SlidingPolicy);

app.MapPost("/user", async (CreateUserInput input, IUserService userService) =>
{
    var createUserResult = await userService.CreateUser(input);
    if (createUserResult.IsFailure)
    {
        throw new Exception();
    }
    return createUserResult.Value;
}).RequireRateLimiting(SlidingPolicy);

app.Run();

// minimal api testing trick
public partial class Program { }