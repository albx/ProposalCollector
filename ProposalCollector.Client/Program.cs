using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ProposalCollector.Client;
using ProposalCollector.Client.Configuration;
using ProposalCollector.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddLocalization();

builder.Services.Configure<CaptchaOptions>(
    options => builder.Configuration.GetSection(nameof(CaptchaOptions)).Bind(options));

builder.Services.AddHttpClient<IProposalServices, ProposalServices>(client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();