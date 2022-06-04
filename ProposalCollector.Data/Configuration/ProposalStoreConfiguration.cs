namespace ProposalCollector.Data.Configuration;

public record ProposalStoreConfiguration
{
    public string ConnectionString { get; set; } = string.Empty;
}
