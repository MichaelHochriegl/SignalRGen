using SignalRGen.Example.Contracts.Client.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// We register the generated SignalR client here.
// These calls allow for a whole suite of configuration values, but we are mostly happy with the defaults.
// Notice that we can chain the generated Client configurations as done with the `ExampleHubClient`
// and the `ChatHubContractClient`.
builder.Services
    .AddExampleHubs(c => c.HubBaseUri = new Uri("http://localhost:5155"))
    .WithExampleHubClient()
    .WithChatHubContractClient(config =>
    {
        // We overwrite the default lifetime for the ChatHubContractClient from `Singleton` to `Scoped`,
        // as each tab will have its own instance of the client.
        config.HubClientLifetime = ServiceLifetime.Scoped;
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();