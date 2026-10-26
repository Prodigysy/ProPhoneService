using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProPhoneService.Application;
using ProPhoneService.Domain.Shared;
using ProPhoneService.Infrastructure.EfCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString) && !builder.Environment.IsDevelopment())
{
    throw new InvalidOperationException(
        "Строка подключения 'Default' не задана; задайте ConnectionStrings__Default в окружении");
}

builder.Services.AddRepositories(connectionString);
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));

builder.Services.AddApplicationServices();

builder.Services.AddOptions<WorkshopOptions>()
    .Bind(builder.Configuration.GetSection("Workshop"))
    .Validate(o => o.Validate() is null, "Некорректные настройки Workshop")
    .ValidateOnStart();

builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<WorkshopOptions>>().Value);

var app = builder.Build();

app.MapGet("/health", () => Results.Text("Healthy"));

app.MapControllers();

if (app.Environment.IsDevelopment() && !string.IsNullOrWhiteSpace(connectionString))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ProPhoneServiceDbContext>();
    db.Database.Migrate();
}
app.Run();
