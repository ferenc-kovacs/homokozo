using Microsoft.Extensions.DependencyInjection;

namespace Homokozo.Data.Test;


public class PostgresUserRepositoryTests
{
    private readonly IRepository<User> _sut;

    public PostgresUserRepositoryTests()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPostgres(new PostgresConfiguration
        {
            ConnectionString = "Host=localhost;Port=5432;Database=homokozo_db;Username=user;Password=password"
        });
        var serviceProvider = serviceCollection.BuildServiceProvider();
        _sut = serviceProvider.GetRequiredService<IRepository<User>>();
    }

    [Fact(Skip = "TODO fix in pipelines")]
    public async Task Create_Read_Update_Delete_Returns_Success()
    {
        // Arrange
        var user = CreateUser();

        // Act
        var createResult = await _sut.Create(user);
        var readResult1 = await _sut.Read(user.Id);
        Assert.Equal(user.Email, readResult1.Value.Email);
        user.Email = "updated";
        var updateResult = await _sut.Update(user);
        var readResult2 = await _sut.Read(user.Id);
        var deleteResult = await _sut.Delete(user.Id);

        // Assert
        Assert.True(createResult.IsSuccess);
        Assert.True(readResult1.IsSuccess);
        Assert.True(updateResult.IsSuccess);
        Assert.True(readResult2.IsSuccess);
        Assert.True(deleteResult.IsSuccess);
        var value = readResult2.Value;
        Assert.Equal(user.Email, value.Email);
        Assert.Equal(user.Name, value.Name);
        Assert.Equal(user.Id, value.Id);
    }

    [Fact(Skip = "TODO: run tests in short lived docker instance")]
    public async Task GetAll_Returns_Success()
    {
        // Arrange
        var user1 = CreateUser();
        await _sut.Create(user1);
        var user2 = CreateUser();
        await _sut.Create(user2);

        // Act 
        var getAllResult = await _sut.GetAll();

        // Assert
        await _sut.Delete(user1.Id);
        await _sut.Delete(user2.Id);
        Assert.Equal(2, getAllResult.Value.Count);
        var value1 = getAllResult.Value[0];
        Assert.Equal(user1.Email, value1.Email);
        Assert.Equal(user1.Name, value1.Name);
        Assert.Equal(user1.Id, value1.Id);
        var value2 = getAllResult.Value[0];
        Assert.Equal(user2.Email, value2.Email);
        Assert.Equal(user2.Name, value2.Name);
        Assert.Equal(user2.Id, value2.Id);
    }

    [Fact(Skip = "TODO fix in pipelines")]
    public async Task Create_Twice_Returns_Duplicate_Error()
    {
        // Arrange
        var user = CreateUser();

        // Act
        var createResult1 = await _sut.Create(user);
        var createResult2 = await _sut.Create(user);

        // Assert
        var deleteResult = await _sut.Delete(user.Id);
        Assert.True(createResult1.IsSuccess);
        Assert.True(createResult2.IsFailure);
        Assert.True(deleteResult.IsSuccess);
    }

    private User CreateUser()
    {
        return new User
        {
            Email = "user@test.eu",
            Name = "user",
            Id = Random.Shared.Next()
        };
    }
}
