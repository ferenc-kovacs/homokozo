using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit.Abstractions;

namespace Homokozo.Data.Test;

public class RateLimitingTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public RateLimitingTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task RateLimitingConfiguration_Works_As_Expected()
    {
        var rateLimitingConfiguration = new RateLimitingConfiguration
        {
            PermitLimit = 10
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                [$"{RateLimitingConfiguration.Path}:{nameof(RateLimitingConfiguration.PermitLimit)}"] = rateLimitingConfiguration.PermitLimit.ToString(),
                [$"{RateLimitingConfiguration.Path}:{nameof(RateLimitingConfiguration.QueueLimit)}"] = rateLimitingConfiguration.QueueLimit.ToString(),
                [$"{RateLimitingConfiguration.Path}:{nameof(RateLimitingConfiguration.QueueProcessingOrder)}"] = rateLimitingConfiguration.QueueProcessingOrder,
                [$"{RateLimitingConfiguration.Path}:{nameof(RateLimitingConfiguration.SegmentsPerWindow)}"] = rateLimitingConfiguration.SegmentsPerWindow.ToString(),
                [$"{RateLimitingConfiguration.Path}:{nameof(RateLimitingConfiguration.WindowSeconds)}"] = rateLimitingConfiguration.WindowSeconds.ToString(),
            })
            .Build();

        using var host = await new HostBuilder()
        .ConfigureWebHost(webBuilder =>
        {
            webBuilder
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    services.AddRateLimiting(configuration);
                    services.AddRouting();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseRateLimiter();

                    app.UseEndpoints(endpointBuilder =>
                    {
                        endpointBuilder
                            .MapGet("/", () => "test")
                            .RequireRateLimiting(RateLimitingConfiguration.SlidingPolicy);
                    });
                });
        })
        .StartAsync();

        var client = host.GetTestClient();
        
        var results = new ConcurrentBag<HttpStatusCode>();
        await Parallel.ForAsync(0, rateLimitingConfiguration.PermitLimit + 1, async (i, _) =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/");
            var response = await client.SendAsync(request);
            _testOutputHelper.WriteLine($"{response.StatusCode}");
            results.Add(response.StatusCode);
        });

        Assert.Equal(1, results.Count(statusCode => statusCode is HttpStatusCode.TooManyRequests));
    }
}