using Art.OnShift.Scheduler.Components;
using Art.OnShift.Scheduler.Data;
using Art.OnShift.Shared.Interfaces;
using Art.OnShift.Scheduler.Middlewares;
using Art.OnShift.Scheduler.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using MudBlazor.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
// Pseudocode:
// 1. Ensure localization is configured for "pt-BR" (already done).
// 2. Set the current thread's culture to "pt-BR" at the start of each request.
// 3. When using date.DayOfWeek, call .ToString() with the "pt-BR" culture to get the localized name.


// Add MudBlazor services
builder.Services.AddMudServices();

builder.Services.AddRazorPages();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), o => o.UseNodaTime()));

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.Bind(Constants.AzureAd, options);
        // TODO - remove this line when token validation issue is fixed.
        options.TokenValidationParameters.ValidateIssuer = false;
    });

builder.Services.Configure<RequestLocalizationOptions>(options =>
    {
        var supportedCultures = new[] { new CultureInfo("pt-BR") };
        options.DefaultRequestCulture = new RequestCulture("pt-BR");
        options.SupportedCultures = supportedCultures;
        options.SupportedUICultures = supportedCultures;
    });

builder.Services.AddAuthorization(options =>
    {
        // Require authentication for all pages by default
        options.FallbackPolicy = options.DefaultPolicy;
    });

builder.Services.AddHttpClient<IExternalApiService, ExternalApiService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5077");
});

builder.Services.AddLocalization();
builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IEventAuditService, EventAuditService>();
builder.Services.AddHostedService<BackgroundEventNotificationService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapRazorPages();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseMiddleware<UserContactCheck>();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapControllers();
app.UseRequestLocalization("pt-BR");
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapControllers();


app.Run();
