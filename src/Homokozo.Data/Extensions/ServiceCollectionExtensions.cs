using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStorage(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        var storageConfiguration = configuration
            .GetSection(StorageConfiguration.Path)
            .Get<StorageConfiguration>();
        ArgumentNullException.ThrowIfNull(storageConfiguration);

        switch (storageConfiguration.Implementation)
        {
            case StorageImplementation.Postgres:
                ArgumentNullException.ThrowIfNull(storageConfiguration.PostgresConfiguration);
                serviceCollection.AddPostgres(storageConfiguration.PostgresConfiguration);
                break;
            case StorageImplementation.AzureBlobStorage:
                ArgumentNullException.ThrowIfNull(storageConfiguration.AzureBlobStorageConfiguration);
                serviceCollection.AddAzureBlobStorage(storageConfiguration.AzureBlobStorageConfiguration);
                break;
            default:
                throw new ArgumentOutOfRangeException($"Unsupported storage implementation: {storageConfiguration.Implementation}");
        }

        serviceCollection.AddScoped<IUserService, UserService>();

        return serviceCollection;
    }

    public static IServiceCollection AddPostgres(
        this IServiceCollection serviceCollection,
        PostgresConfiguration postgresConfiguration)
    {
        serviceCollection.AddDbContext<HomokozoDbContext>(options =>
        {
            // TODO implement more credential options, for example DefaultAzureCredentials, ManagedIdentity etc
            options.UseNpgsql(postgresConfiguration.ConnectionString);
        });

        serviceCollection.AddScoped<IRepository<User>, PostgresUserRepository>();

        return serviceCollection;
    }

    private static IServiceCollection AddAzureBlobStorage(
        this IServiceCollection serviceCollection,
        AzureBlobStorageConfiguration azureBlobStorageConfiguration)
    {
        throw new NotImplementedException();

        serviceCollection.AddSingleton<IRepository<User>, AzureBlobStorageUserRepository>();

        return serviceCollection;
    }

    public static IServiceCollection AddRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rateLimitingConfiguration = configuration
            .GetSection(RateLimitingConfiguration.Path)
            .Get<RateLimitingConfiguration>();

        if (rateLimitingConfiguration is null)
        {
            return services;
        }

        // https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit-samples?view=aspnetcore-9.0
        services.AddRateLimiter(options =>
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

            options.AddPolicy(RateLimitingConfiguration.SlidingPolicy, context =>
            {
                var userName = "anonymous user";
                if (context.User.Identity is { IsAuthenticated: true })
                {
                    userName = context.User.Identity.Name;
                }
                return RateLimitPartition.GetSlidingWindowLimiter(userName, _ =>
                new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = rateLimitingConfiguration.PermitLimit,
                    QueueLimit = rateLimitingConfiguration.QueueLimit,
                    QueueProcessingOrder =
                        Enum.TryParse<QueueProcessingOrder>(rateLimitingConfiguration.QueueProcessingOrder, out var queueProcessingOrder)
                        ? queueProcessingOrder
                        : QueueProcessingOrder.OldestFirst,
                    SegmentsPerWindow = rateLimitingConfiguration.SegmentsPerWindow,
                    Window = TimeSpan.FromSeconds(rateLimitingConfiguration.WindowSeconds),
                });
            });
        });

        return services;
    }
}