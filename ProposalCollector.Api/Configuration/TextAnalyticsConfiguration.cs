namespace ProposalCollector.Api.Configuration;

public record TextAnalyticsConfiguration
{
    public string Endpoint { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;
}
