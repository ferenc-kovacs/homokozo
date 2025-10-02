using Microsoft.EntityFrameworkCore;

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
}