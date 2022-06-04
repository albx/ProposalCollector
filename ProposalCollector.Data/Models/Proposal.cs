namespace ProposalCollector.Data.Models;

public class Proposal
{
    public Guid Id { get; set; }

    public string AuthorNickname { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime SubmittedAt { get; set; }
}
