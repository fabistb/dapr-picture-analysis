using FileService.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddDaprClient();
builder.Services.AddControllers().AddDapr();
builder.Services.AddHealthChecks();

builder.Services.AddTransient<IFileService, FileService.Services.FileService>();

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
app.UseCloudEvents();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/health");
    endpoints.MapControllers();
});

app.Run();