using Savorboard.CAP.InMemoryMessageQueue;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

builder.Services.AddCap(x =>
{
    x.UseInMemoryMessageQueue();
    x.UseInMemoryStorage();
    x.UseDashboard();
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseAuthorization();
app.MapControllers();
app.Run();
