using Microsoft.EntityFrameworkCore;

using Todozra.Api;
using Todozra.Db.Todo;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TodoDbContext>(options =>
{
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    options.UseSqlite("TodozraDb");
});

var app = builder.Build();

app.MapAllEndpointsFromAssembly(typeof(IEndPoint).Assembly);

app.UseHttpsRedirection();

app.Run();