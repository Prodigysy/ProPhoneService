using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ProPhoneService.Api.Tests;

/// <summary>
/// Поведение Program.cs на старте: вне Development строка подключения обязательна,
/// в Development она может отсутствовать (endpoint-тесты), миграция пропускается
/// </summary>
public class ProgramStartupTests
{
    private const string ConnectionStringEnvKey = "ConnectionStrings__Default";

    [Fact]
    public void Startup_ProductionWithoutConnectionString_FailsFast()
    {
        Environment.SetEnvironmentVariable(ConnectionStringEnvKey, null);

        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(host =>
            host.UseSetting(WebHostDefaults.EnvironmentKey, "Production"));

        var exception = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());

        Assert.Contains("Строка подключения", exception.Message);
    }

    [Fact]
    public async Task Startup_DevelopmentWithoutConnectionString_StartsAndSkipsMigrate()
    {
        Environment.SetEnvironmentVariable(ConnectionStringEnvKey, "");
        try
        {
            using var client = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(host =>
                    host.UseSetting(WebHostDefaults.EnvironmentKey, "Development"))
                .CreateClient();

            var response = await client.GetAsync("/health");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        finally
        {
            Environment.SetEnvironmentVariable(ConnectionStringEnvKey, null);
        }
    }

    [Fact]
    public async Task Startup_ProductionWithConnectionString_Starts()
    {
        Environment.SetEnvironmentVariable(ConnectionStringEnvKey, "Host=x");
        try
        {
            using var client = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(host =>
                    host.UseSetting(WebHostDefaults.EnvironmentKey, "Production"))
                .CreateClient();

            var response = await client.GetAsync("/health");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        finally
        {
            Environment.SetEnvironmentVariable(ConnectionStringEnvKey, null);
        }
    }
}
