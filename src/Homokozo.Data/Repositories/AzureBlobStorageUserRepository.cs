using CSharpFunctionalExtensions;

public class AzureBlobStorageUserRepository : IRepository<User>
{
    public AzureBlobStorageUserRepository()
    {
        
    }

    public Task<Result<User, StorageError>> Create(User entity)
    {
        throw new NotImplementedException();
    }

    public Task<UnitResult<StorageError>> Delete(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IReadOnlyList<User>, StorageError>> GetAll()
    {
        throw new NotImplementedException();
    }

    public Task<Result<User, StorageError>> Read(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Result<User, StorageError>> Update(User entity)
    {
        throw new NotImplementedException();
    }
}