using KITT.Web.ReCaptcha.Http.v3;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProposalCollector.Api.Configuration;
using ProposalCollector.Api.Services;
using ProposalCollector.Data;
using ProposalCollector.Data.Configuration;

var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();

builder.Services.AddMvc();
builder.Services.AddHttpClient();

builder.Services.Configure<ProposalStoreConfiguration>(options =>
{
    options.ConnectionString = builder.Configuration["KittConnectionString"]!;
});

builder.Services.Configure<TextAnalyticsConfiguration>(options =>
{
    options.Endpoint = builder.Configuration["TextAnalyticsEndpoint"]!;
    options.Key = builder.Configuration["TextAnalyticsKey"]!;
});

builder.Services.AddReCaptchaV3HttpClient(options => options.SecretKey = builder.Configuration["CaptchaSecret"]!);
builder.Services.AddScoped<ITextAnalyticsService, AzureTextAnalyticsService>();

builder.Services.AddScoped<IProposalStore, ProposalStore>();

var host = builder.Build();
host.Run();