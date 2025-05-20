using DevHabit.Api;
using DevHabit.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder
    .AddControllers()
    .AddErrorHandling()
    .AddDatabase()
    .AddObservability()
    .AddApplicationServices();

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
