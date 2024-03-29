using VitalTrack.Core;
using VitalTrack.Core.Services;
using VitalTrack.Infrastructure;
using VitalTrack.Web;
using VitalTrack.Web.Concerns;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Postgres");
var templateFilePath = builder.Configuration.GetValue<string>("TemplatePath");

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddVitalTrackCore();
builder.Services.AddVitalTrackInfrastructure(connectionString);
builder.Services.AddProblemDetails(); // TODO: Probably worth it to add custom problem details here eventually
var app = builder.Build();

await using var providerScope = app.Services.CreateAsyncScope();
var logger = providerScope.ServiceProvider.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Initializing API routes");

app.MapGroup("/api/player/{playerName}")
    .MapVitalTrackRoutes()
    .AddEndpointFilter<ExistingPlayerFilter>();

logger.LogInformation("API routes initialized, seeding players from template");

// Next, we'll seed Briv into the player repository
var playerRepository = providerScope.ServiceProvider.GetRequiredService<IPlayerRepository>();

logger.LogInformation("Seeding player from template path {playerTemplatePath}", templateFilePath);

await playerRepository.SeedPlayerFromTemplateAsync(templateFilePath!);

logger.LogInformation("Player seeded from template path, now listening on default port");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

await app.RunAsync();
