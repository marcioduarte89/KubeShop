using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Order.Api.Infrastructure;
using Order.Api.Services;

const string API_NAME = "orders";

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
var isAspire = Environment.GetEnvironmentVariable("RUNNING_UNDER_ASPIRE") == "true";

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.CustomSchemaIds(a => a.FullName?.Replace("+", "."));
});

builder.Services.AddHttpClient<IProductService, ProductService>(client =>
{
    client.BaseAddress = new Uri(!isAspire ? builder.Configuration["ProductApiUrl"] : $"{builder.Configuration["services:ProductApi:https:0"]}/api/");
});

builder.Services.AddHealthChecks();

builder.Services.AddDbContext<OrderDbContext>(options =>
 options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<OrderDbContext>("DbChecks", tags: ["dbcheck"]);

if (isAspire)
{
    builder.Services.AddSingleton(TracerProvider.Default.GetTracer(API_NAME));

    builder.Logging.AddOpenTelemetry(options =>
    {
        options
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService(API_NAME))
            //.AddConsoleExporter()
            /*.AddOtlpExporter()*/;
    });

    builder.Services.AddOpenTelemetry()
          .ConfigureResource(resource => resource.AddService(API_NAME))
          .WithTracing(tracing => tracing
              .SetSampler(new AlwaysOnSampler())
              .AddSource(API_NAME)
              .AddAspNetCoreInstrumentation()
              .AddHttpClientInstrumentation()
              //.AddConsoleExporter()
              .AddNpgsql()
              /*.AddOtlpExporter()*/)
          .WithMetrics(metrics => metrics
              .AddAspNetCoreInstrumentation()
              //.AddConsoleExporter()
              .AddPrometheusExporter());
}
else
{
    builder.Logging.AddOpenTelemetry(options =>
    {
        options
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService(API_NAME))
            .AddConsoleExporter()
            .AddOtlpExporter();
    });

    builder.Services.AddOpenTelemetry()
          .ConfigureResource(resource => resource.AddService(API_NAME))
          .WithTracing(tracing => tracing
              .SetSampler(new AlwaysOnSampler())
              .AddSource(API_NAME)
              .AddAspNetCoreInstrumentation()
              .AddHttpClientInstrumentation()
              .AddConsoleExporter()
              .AddNpgsql()
              .AddOtlpExporter())
          .WithMetrics(metrics => metrics
              .AddAspNetCoreInstrumentation()
              .AddConsoleExporter()
              .AddPrometheusExporter());
}

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.UseHttpsRedirection();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = healthCheck => healthCheck.Tags.Contains("dbcheck")
});

app.UseAuthorization();

app.MapControllers();

// Running any migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
