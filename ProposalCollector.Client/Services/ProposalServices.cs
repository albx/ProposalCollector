using ProposalCollector.Shared;
using System.Net.Http.Json;

namespace ProposalCollector.Client.Services;

public class ProposalServices : IProposalServices
{
    private readonly ILogger<ProposalServices> _logger;

    public HttpClient Client { get; }

    public ProposalServices(HttpClient client, ILogger<ProposalServices> logger)
    {
        Client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SubmitProposalAsync(ProposalModel proposal)
    {
        var response = await Client.PostAsJsonAsync("api/SubmitProposal", proposal);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Error submitting proposal {Title}, {Description}: {StatusCode}",
                proposal.Title,
                proposal.Description,
                response.StatusCode);

            throw new Exception("Error submitting proposal");
        }
    }
}
