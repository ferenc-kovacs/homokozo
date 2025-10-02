
using System.Collections.Immutable;
using CSharpFunctionalExtensions;

public class PostgresUserRepository : IRepository<User>
{
    private readonly HomokozoDbContext _appDbContext;

    public PostgresUserRepository(HomokozoDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<Result<User, StorageError>> Create(User entity)
    {
        _appDbContext.Users.Add(entity);
        try
        {
            await _appDbContext.SaveChangesAsync();
        }
        catch (Exception)
        {

            return StorageError.Duplicate;
        }
        return entity;
    }

    public async Task<UnitResult<StorageError>> Delete(int id)
    {
        var user = _appDbContext.Users.Find(id);
        if (user is null)
        {
            return StorageError.NotFound;
        }
        _appDbContext.Users.Remove(user);
        await _appDbContext.SaveChangesAsync();
        return UnitResult.Success<StorageError>();
    }

    public Task<Result<IReadOnlyList<User>, StorageError>> GetAll()
    {
        return Task.FromResult(Result.Success<IReadOnlyList<User>, StorageError>([.. _appDbContext.Users]));
    }

    public async Task<Result<User, StorageError>> Read(int id)
    {
        var user = await _appDbContext.Users.FindAsync(id);
        if (user is null)
        {
            return StorageError.NotFound;
        }
        return user;
    }

    public async Task<Result<User, StorageError>> Update(User entity)
    {
        var user = _appDbContext.Users.Find(entity.Id);
        if (user is null)
        {
            return StorageError.NotFound;
        }

        user.Email = entity.Email;
        user.Name = entity.Name;

        await _appDbContext.SaveChangesAsync();
        return user;
    }
}