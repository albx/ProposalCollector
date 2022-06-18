using Microsoft.Extensions.Options;
using ProposalCollector.Api.Configuration;
using ProposalCollector.Api.Models;
using System.Text.Json;

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

    public async Task<CaptchaVerificationResponse> VerifyAsync(string captchaResponse)
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

        var verificationResponse = JsonSerializer.Deserialize<CaptchaVerificationResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return verificationResponse ?? CaptchaVerificationResponse.Failed;
    }
}
