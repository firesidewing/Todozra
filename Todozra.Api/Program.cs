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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
        db.Database.Migrate();
    }
}

app.MapEndpoints();

app.UseHttpsRedirection();

app.Run();