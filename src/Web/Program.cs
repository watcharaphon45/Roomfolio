using Roomfolio.Application;
using Roomfolio.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add Clean Architecture layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add API controllers / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/v1/health", () => Results.Ok(new
{
    status = "healthy",
    service = "Roomfolio.Web",
    timestamp = DateTimeOffset.UtcNow
}))
.WithName("HealthCheck")
.WithOpenApi();

app.Run();
