using CSharpFunctionalExtensions;

public interface IRepository<TEntity> where TEntity : EntityBase
{
    Task<Result<TEntity, StorageError>> Create(TEntity entity);
    Task<Result<TEntity, StorageError>> Read(int id);
    Task<Result<TEntity, StorageError>> Update(TEntity entity);
    Task<UnitResult<StorageError>> Delete(int id);
    Task<Result<IReadOnlyList<TEntity>, StorageError>> GetAll();
}