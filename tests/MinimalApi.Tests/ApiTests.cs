using Microsoft.AspNetCore.Mvc.Testing;
namespace MinimalApi.Tests;
public class ApiTests
{
    [Fact]
    public async Task ApplicationRootEndPoint()
    {
        await using var application = new WebApplicationFactory<Program>();
        using var client = application.CreateClient();

        var response = await client.GetStringAsync("/");
  
        Assert.Equal("Hello World!", response);
    }
}