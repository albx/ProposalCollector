namespace ProposalCollector.Api.Configuration;

public record CaptchaConfiguration
{
    public string CaptchaSecret { get; set; } = string.Empty;

    public string VerificationEndpoint { get; set; } = string.Empty;
}
