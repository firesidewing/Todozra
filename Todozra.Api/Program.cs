using Azure.Monitor.OpenTelemetry.Exporter;

using Microsoft.EntityFrameworkCore;

using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Todozra.Api;
using Todozra.Db.Todo;

var builder = WebApplication.CreateBuilder(args);

const string serviceName = "Todozra.Api";

builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
        .AddOtlpExporter();
});

var openTel = builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter());

if (builder.Environment.IsProduction() &&
    !string.IsNullOrEmpty(builder.Configuration.GetValue<string?>("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    openTel.UseAzureMonitorExporter();
}

builder.Services.AddDbContext<TodoDbContext>(options =>
{
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    options.UseSqlite(builder.Configuration.GetConnectionString("TodoDb"));
});

builder.Services.AddEndpoints(typeof(IEndPoint).Assembly);

builder.Services.AddProblemDetails();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    db.Database.Migrate();
}

app.MapEndpoints();

app.MapControllers();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Run();