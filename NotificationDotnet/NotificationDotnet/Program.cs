using NotificationDotnet.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddDaprClient();
builder.Services.AddControllers().AddDapr();
builder.Services.AddHealthChecks();

builder.Services.AddTransient<INotificationService, NotificationService>();

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
    endpoints.MapSubscribeHandler();
});

app.Run();