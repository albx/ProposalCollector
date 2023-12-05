using KITT.Web.ReCaptcha.Blazor.v3;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ProposalCollector.Client;
using ProposalCollector.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.RootComponents.RegisterReCaptchaScript();

builder.Services.AddLocalization();

builder.Services.AddReCaptchaV3(
    options => options.SiteKey = builder.Configuration["CaptchaOptions:SiteKey"]!);

builder.Services.AddHttpClient<IProposalServices, ProposalServices>(client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();