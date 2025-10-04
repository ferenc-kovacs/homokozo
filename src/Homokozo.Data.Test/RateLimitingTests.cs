using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace Homokozo.Data.Test;

public class RateLimitingTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public RateLimitingTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact(Skip = "TODO fix ratelimiting")]
    public async Task Foo()
    {
        await using var app = new WebApplicationFactory<Program>();
        using var client = app.CreateClient();

        await Parallel.ForAsync(0, 11, async (_, _) =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/user/123");
            var response = await client.SendAsync(request);
            _testOutputHelper.WriteLine($"{response.StatusCode}");
        });
    }
}