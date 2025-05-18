using DevHabit.Api.Database;
using DevHabit.Api.Database.SortMapping;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using DevHabit.Api.Extensions;
using DevHabit.Api.Middlewares;
using DevHabit.Api.Tools;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json.Serialization;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
})
.AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = 
    new CamelCasePropertyNamesContractResolver())
.AddXmlSerializerFormatters();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
    };
});
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database"),
    npgsqlOptions =>
    {
        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Application);
    });
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
    .WithTracing(tracing => tracing
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddNpgsql())
    .WithMetrics(metrics => metrics
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation())
    .UseOtlpExporter();

builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeScopes = true;
    options.IncludeFormattedMessage = true;
});

builder.Services.AddTransient<SortMappingProvider>();
builder.Services.AddSingleton<ISortMapptingDefinition, SortMappingDefinition<HabitDto, Habit>>(_ =>
    HabitMappings.SortMapping);
builder.Services.AddTransient<DataShapingTool>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.ApplyMigrationsAsync();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.MapControllers();
await app.RunAsync();
