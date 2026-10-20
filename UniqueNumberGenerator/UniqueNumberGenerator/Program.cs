using Asp.Versioning;
using UniqueNumberGenerator.Actors;
using UniqueNumberGenerator.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDaprClient();
builder.Services.AddControllers().AddDapr();
builder.Services.AddHealthChecks();

builder.Services.AddActors(options =>
{
    options.Actors.RegisterActor<UniqueNumberActor>();
});

builder.Services.AddTransient<IUniqueNumberService, UniqueNumberService>();

builder.Services.AddApiVersioning(v =>
{
    v.ReportApiVersions = true;
    v.AssumeDefaultVersionWhenUnspecified = true;
    v.DefaultApiVersion = new ApiVersion(1, 0);
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDaprSidekick(builder.Configuration);
}

// Configure the HTTP request pipeline
var app = builder.Build();

app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();
app.MapActorsHandlers();

app.Run();
