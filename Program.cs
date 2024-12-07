using InfoedukaScraper;
using InfoedukaScraper.Components;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();

builder.Services.AddHttpClient();

builder.Services.AddScoped<IeAuthentication>();
builder.Services.AddScoped<FetchData>(sp =>
{
    Console.WriteLine("Registering FetchData");
    return new FetchData(sp.GetRequiredService<HttpClient>());
});

foreach (var service in builder.Services)
{
    Console.WriteLine($"Service: {service.ServiceType.FullName}");
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
