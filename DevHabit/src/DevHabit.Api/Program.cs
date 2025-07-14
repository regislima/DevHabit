using DevHabit.Api;
using DevHabit.Api.Extensions;
using DevHabit.Api.Middlewares;
using DevHabit.Api.Settings;

var builder = WebApplication.CreateBuilder(args);
builder
    .AddControllers()
    .AddErrorHandling()
    .AddDatabase()
    .AddObservability()
    .AddApplicationServices()
    .AddAuthenticationServices()
    .AddBackgroundJobs()
    .AddCorsPolicy();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.ApplyMigrationsAsync();
    await app.SeedinitialDataAsync();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseCors(CorsOptions.PolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ETagMiddleware>();
app.MapControllers();
await app.RunAsync();
