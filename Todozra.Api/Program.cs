using Microsoft.EntityFrameworkCore;

using Todozra.Api;
using Todozra.Db.Todo;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TodoDbContext>(options =>
{
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    options.UseSqlite("TodozraDb");
});

builder.Services.AddEndpoints(typeof(IEndPoint).Assembly);

var app = builder.Build();

app.MapEndpoints();

app.UseHttpsRedirection();

app.Run();