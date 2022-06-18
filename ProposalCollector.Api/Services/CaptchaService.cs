using Microsoft.Extensions.Options;
using ProposalCollector.Api.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProposalCollector.Api.Services;

public class CaptchaService
{
    private readonly CaptchaConfiguration _captchaConfiguration;
    private readonly IHttpClientFactory _httpClientFactory;

    public CaptchaService(IOptions<CaptchaConfiguration> captchaConfiguration, IHttpClientFactory httpClientFactory)
    {
        _captchaConfiguration = captchaConfiguration?.Value ?? throw new ArgumentNullException(nameof(captchaConfiguration));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task<VerificationResponse> VerifyAsync(string captchaResponse)
    {
        var client = _httpClientFactory.CreateClient();

        var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["secret"] = _captchaConfiguration.CaptchaSecret,
            ["response"] = captchaResponse
        });

        var response = await client.PostAsync(_captchaConfiguration.VerificationEndpoint, requestContent);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(responseContent, null, response.StatusCode);
        }

        var verificationResponse = JsonSerializer.Deserialize<VerificationResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return verificationResponse ?? VerificationResponse.Failed;
    }
}

public record VerificationResponse
{
    public bool Success { get; set; }

    [JsonIgnore]
    public static VerificationResponse Failed { get; } = new VerificationResponse { Success = false };
}
