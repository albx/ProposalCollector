using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProposalCollector.Data;
using ProposalCollector.Data.Configuration;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.Configure<ProposalStoreConfiguration>(options =>
        {
            options.ConnectionString = context.Configuration["KittConnectionString"];
        });

        services.AddScoped<IProposalStore, ProposalStore>();
    })
    .Build();

host.Run();