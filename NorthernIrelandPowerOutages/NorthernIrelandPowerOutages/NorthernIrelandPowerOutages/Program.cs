using Infrastructure;
using Infrastructure.ProjectSettings;
using Microsoft.AspNetCore.StaticFiles;
using NorthernIrelandPowerOutages;
using NorthernIrelandPowerOutages.Client.Pages;
using NorthernIrelandPowerOutages.Components;
using NorthernIrelandPowerOutages.Startup;

var builder = WebApplication.CreateBuilder(args);

builder.AddUserSecrets();

builder.AddDependencies();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddBlazorBootstrap();

builder.Services.AddHttpClient();

var app = builder.Build();
app.AddStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Counter).Assembly);

app.Run();
