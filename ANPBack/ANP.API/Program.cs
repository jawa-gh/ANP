using System.Text.Json.Serialization;
using ANP.API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
    );
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString =
    builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "Connection string 'Default' is not configured. Set it with: "
            + "dotnet user-secrets set \"ConnectionStrings:Default\" \"Host=localhost;Port=5432;Database=anpdb;Username=postgres;Password=...\""
    );

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

// Allow the Angular dev server (and SSR server) to call this API from the browser.
const string AngularCors = "angular-dev";
builder.Services.AddCors(options =>
    options.AddPolicy(
        AngularCors,
        policy =>
            policy
                .WithOrigins("http://localhost:4200", "http://localhost:4000")
                .AllowAnyHeader()
                .AllowAnyMethod()
    )
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Demo: create the schema and seed sample data on startup.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DbSeeder.SeedAsync(db);
}

app.UseHttpsRedirection();

app.UseCors(AngularCors);

app.UseAuthorization();

app.MapControllers();

app.Run();
