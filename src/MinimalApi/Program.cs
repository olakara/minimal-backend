using System.Reflection;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);

ConfigureLogging();
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/", (ILoggerFactory loggerFactory) =>{
    var logger = loggerFactory.CreateLogger("Root");
    logger.LogInformation("Application Root called!");
    return "Hello World!";
});

app.MapGet("/version",(ILoggerFactory loggerFactory) =>{
    var logger = loggerFactory.CreateLogger("Version");
    logger.LogInformation("Application Version API!");
    return Results.Ok("Version: 0.3.0!");
});

app.MapGet("/delay",(ILoggerFactory loggerFactory) =>{
    var logger = loggerFactory.CreateLogger("Delayed");
    logger.LogInformation("Delayed API!");
    var randomGenerator = new Random();
    var delay  = randomGenerator.Next(1000, 5000);
    logger.LogWarning($"Delaying for {delay} seconds");    
    Thread.Sleep(delay);
    return Results.Ok(new {
        Message = "Delay of " + delay
    });
});


app.Run();

void ConfigureLogging()
{
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile(
            $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
            optional: true)
        .Build();

    Log.Logger = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .Enrich.WithExceptionDetails()
        .WriteTo.Debug()
        .WriteTo.Console()
        .WriteTo.Elasticsearch(ConfigureElasticSink(configuration, environment))
        .Enrich.WithProperty("Environment", environment)
        .ReadFrom.Configuration(configuration)
        .CreateLogger();
}

ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationRoot configuration, string environment)
{
    var indexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}-{environment?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}";
    System.Console.WriteLine("Format: " +  indexFormat);
    
    return new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration:Uri"]))
    {
        AutoRegisterTemplate = true,
        IndexFormat = indexFormat

    };
}

public partial class Program { }
