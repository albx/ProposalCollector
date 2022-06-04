namespace ProposalCollector.Data;

public interface IProposalStore
{
    Task SubmitNewProposal(string authorNickname, string title, string description);
}
