using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NumberGenerator;
using System.Net;
using Testcontainers.Redis;

namespace NumberGenerator.IntegrationTests;

[TestClass]
public class NumberGeneratorIntegrationTests
{
    private static RedisContainer? _redisContainer;
    private static WebApplicationFactory<Program>? _factory;
    private static HttpClient? _client;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        // Start Redis container
        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithPortBinding(6379, true)
            .Build();

        await _redisContainer.StartAsync();

        // Create WebApplicationFactory
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // The application will use the Redis container
                    // Dapr will handle the connection via the actor state store
                });
            });

        _client = _factory.CreateClient();
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        _client?.Dispose();
        
        if (_factory != null)
        {
            await _factory.DisposeAsync();
        }
        
        if (_redisContainer != null)
        {
            await _redisContainer.DisposeAsync();
        }
    }

    [TestMethod]
    public async Task GetNextNumber_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client!.GetAsync("/api/v1.0/Number");

        // Assert
        Assert.IsTrue(response.IsSuccessStatusCode);
    }

    [TestMethod]
    public async Task GetNextNumber_ReturnsNumericValue()
    {
        // Act
        var response = await _client!.GetAsync("/api/v1.0/Number");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.IsTrue(long.TryParse(content, out var number));
        Assert.IsTrue(number > 0);
    }

    [TestMethod]
    public async Task GetNextNumber_ReturnsIncrementingNumbers()
    {
        // Act
        var response1 = await _client!.GetAsync("/api/v1.0/Number");
        var number1 = long.Parse(await response1.Content.ReadAsStringAsync());

        var response2 = await _client!.GetAsync("/api/v1.0/Number");
        var number2 = long.Parse(await response2.Content.ReadAsStringAsync());

        var response3 = await _client!.GetAsync("/api/v1.0/Number");
        var number3 = long.Parse(await response3.Content.ReadAsStringAsync());

        // Assert
        Assert.IsGreaterThan(number2, number1, $"Expected {number2} > {number1}");
        Assert.IsGreaterThan(number3, number2, $"Expected {number3} > {number2}");
    }
}
