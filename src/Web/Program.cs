using Roomfolio.Application;
using Roomfolio.Infrastructure;
using Roomfolio.Infrastructure.Persistence;
using Roomfolio.Web.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add Clean Architecture layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add CORS for Frontend dev server
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add API controllers / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Run DB migrations and Seed initial data in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<RoomfolioDbContext>();
    await DatabaseSeeder.SeedAsync(db);
}

app.UseCors();
app.UseHttpsRedirection();

// Health Check
app.MapGet("/api/v1/health", () => Results.Ok(new
{
    status = "healthy",
    service = "Roomfolio.Web",
    timestamp = DateTimeOffset.UtcNow
}))
.WithName("HealthCheck")
.WithOpenApi();

// Map Feature Endpoints
app.MapUnitEndpoints();
app.MapOwnerEndpoints();
app.MapTenantEndpoints();
app.MapLeaseEndpoints();
app.MapRentCollectionEndpoints();
app.MapOwnerSettlementEndpoints();
app.MapDashboardEndpoints();
app.MapExpenseEndpoints();
app.MapSecurityDepositEndpoints();
app.MapAgencyFeeEndpoints();
app.MapSettingsEndpoints();
app.MapReportEndpoints();

app.Run();
