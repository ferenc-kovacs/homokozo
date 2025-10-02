using CSharpFunctionalExtensions;

/// <summary>
/// Perform business logic related to Users.
/// </summary>
public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;

    public UserService(
        IRepository<User> userRepository
        /*TODO logger*/)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserOutput, ServiceError>> CreateUser(CreateUserInput input)
    {
        var user = ToUser(input);
        var createResult = await _userRepository.Create(user);
        return createResult
            .Map(FromUser)
            .MapError(ToServiceError);
    }

    public async Task<UnitResult<ServiceError>> DeleteUser(GetUserInput input)
    {
        var result = await _userRepository.Delete(input.Id);
        return result.MapError(ToServiceError);
    }

    public async Task<Result<UserOutput, ServiceError>> GetUser(GetUserInput input)
    {
        var readResult = await _userRepository.Read(input.Id);
        return readResult
            .Map(FromUser)
            .MapError(ToServiceError);
    }

    private User ToUser(CreateUserInput input) => new User
    {
        Id = input.Id,
        Email = input.Email,
        Name = input.Name,
    };

    private UserOutput FromUser(User user) => new UserOutput
    {
        Email = user.Email,
        Name = user.Name,
        Id = user.Id,
    };

    private ServiceError ToServiceError(StorageError storageError) => storageError switch
    {
        StorageError.Duplicate => ServiceError.InvalidInput,
        StorageError.NotFound => ServiceError.InvalidInput,
        StorageError.Unknown => ServiceError.InternalError,
        _ => throw new ArgumentOutOfRangeException()
    };
}
