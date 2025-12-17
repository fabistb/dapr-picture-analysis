using NumberGenerator.Actors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Dapr services
builder.Services.AddActors(options =>
{
    options.Actors.RegisterActor<UniqueNumberActor>();
});

// Add Dapr Sidekick for development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDaprSidekick(builder.Configuration);
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapActorsHandlers();

app.Run();

// Make the Program class accessible for testing
public partial class Program { }

