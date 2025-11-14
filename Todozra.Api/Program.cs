using Microsoft.EntityFrameworkCore;

using Todozra.Api;
using Todozra.Db.Todo;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TodoDbContext>(options =>
{
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    options.UseSqlite(builder.Configuration.GetConnectionString("TodoDb"));
});

builder.Services.AddEndpoints(typeof(IEndPoint).Assembly);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
}

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
        db.Database.Migrate();
    }
}

app.MapEndpoints();

app.MapControllers();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Run();