using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProposalCollector.Api.Configuration;
using ProposalCollector.Api.Services;
using ProposalCollector.Data;
using ProposalCollector.Data.Configuration;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient();

        services.Configure<ProposalStoreConfiguration>(options =>
        {
            options.ConnectionString = context.Configuration["KittConnectionString"];
        });

        services.Configure<CaptchaConfiguration>(options =>
        {
            options.CaptchaSecret = context.Configuration["CaptchaSecret"];
            options.VerificationEndpoint = context.Configuration["CaptchaVerificationEndpoint"];
        });

        services.AddScoped<CaptchaService>();

        services.AddScoped<IProposalStore, ProposalStore>();
    })
    .Build();

host.Run();