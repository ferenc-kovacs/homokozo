using CSharpFunctionalExtensions;

public interface IUserService
{
    Task<Result<UserOutput, ServiceError>> CreateUser(CreateUserInput input);
    Task<Result<UserOutput, ServiceError>> GetUser(GetUserInput input);
    Task<UnitResult<ServiceError>> DeleteUser(GetUserInput input);
}

public enum ServiceError
{
    InvalidInput,
    InternalError,
}