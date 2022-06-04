using ProposalCollector.Shared;

namespace ProposalCollector.Client.Services;

public interface IProposalServices
{
    Task SubmitProposalAsync(ProposalModel proposal);
}
