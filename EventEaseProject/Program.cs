using EventEaseProject.Components;
using EventEaseProject.Models;
using EventEaseProject.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

var builder = WebApplication.CreateBuilder(args);
var staticAssetCacheControl = builder.Configuration["StaticAssets:CacheControl"]
    ?? throw new InvalidOperationException("StaticAssets:CacheControl must be configured.");

builder.Services.AddDataProtection();
builder.Services.AddResponseCompression(options => options.EnableForHttps = true);
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.Configure<EventCatalogOptions>(builder.Configuration.GetSection("Events"));
builder.Services.AddSingleton<IEventService, EventService>();
builder.Services.AddSingleton<IRegistrationService, RegistrationService>();
builder.Services.AddSingleton<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<IProtectedSessionStore, ProtectedSessionStore>();
builder.Services.AddScoped<UserSessionService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseResponseCompression();
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
        context.Context.Response.Headers.CacheControl = staticAssetCacheControl
});

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
