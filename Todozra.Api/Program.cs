using Microsoft.EntityFrameworkCore;

using Todozra.Db.Todo;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

builder.Services.AddDbContext<TodoDbContext>(options =>
{
    options.UseSqlite("TodozraDb");
});

app.UseHttpsRedirection();