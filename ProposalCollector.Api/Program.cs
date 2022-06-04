using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProposalCollector.Data;
using ProposalCollector.Data.Configuration;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.Configure<ProposalStoreConfiguration>(context.Configuration.GetSection(nameof(ProposalStoreConfiguration)));
        services.AddScoped<IProposalStore, ProposalStore>();
    })
    .Build();

host.Run();