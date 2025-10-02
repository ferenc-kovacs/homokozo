public class StorageConfiguration
{
    public const string Path = "Storage";

    public StorageImplementation Implementation { get; set; } = StorageImplementation.Postgres;

    public AzureBlobStorageConfiguration? AzureBlobStorageConfiguration { get; set; } = null;

    public PostgresConfiguration? PostgresConfiguration { get; set; } = null;
}