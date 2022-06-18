using System.Text.Json.Serialization;

namespace ProposalCollector.Api.Models;

public record CaptchaVerificationResponse
{
    public bool Success { get; set; }

    [JsonIgnore]
    public static CaptchaVerificationResponse Failed { get; } = new CaptchaVerificationResponse { Success = false };
}
