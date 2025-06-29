using DevHabit.Api;
using DevHabit.Api.Extensions;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder
    .AddApiServices()
    .AddErrorHandling()
    .AddDatabase()
    .AddObservability()
    .AddApplicationServices();


WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.ApplyMigrationsAsync();
    await app.SeedDataAsync();

}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.MapControllers();

// Fix for S6966: Await RunAsync instead.
await app.RunAsync();
