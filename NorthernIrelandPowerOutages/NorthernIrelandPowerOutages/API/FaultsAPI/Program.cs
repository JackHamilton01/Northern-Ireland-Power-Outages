using FaultsAPI.EndPoints;
using FaultsAPI.Startup;
using System.Text.Json.Serialization;
using System.Text.Json;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

builder.AddDependencies();

WebApplication? app = builder.Build();

app.UseOpenApi();

app.UseHttpsRedirection();

app.ApplyCorsConfig();

app.MapAllHealthChecks();

app.AddRootEndpoints();
app.AddErrorEndpoints();
app.AddFaultEndpoints();

app.Run();

