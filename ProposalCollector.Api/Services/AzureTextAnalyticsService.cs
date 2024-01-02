using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.Extensions.Options;
using ProposalCollector.Api.Configuration;
using ProposalCollector.Api.Models;

namespace ProposalCollector.Api.Services;

public class AzureTextAnalyticsService : ITextAnalyticsService
{
    private readonly TextAnalyticsConfiguration _configuration;

    private readonly TextAnalyticsClient _client;

    public AzureTextAnalyticsService(IOptions<TextAnalyticsConfiguration> configuration)
    {
        _configuration = configuration?.Value ?? throw new ArgumentNullException(nameof(configuration));
        _client = new TextAnalyticsClient(
            new Uri(_configuration.Endpoint),
            new AzureKeyCredential(_configuration.Key));
    }

    public async Task<TextAnalyticsResponse> AnalyzeAsync(string text)
    {
        var language = "it";
        var detectedLanguageResponse = await _client.DetectLanguageAsync(text);
        if (detectedLanguageResponse is not null)
        {
            language = detectedLanguageResponse.Value.Iso6391Name;
        }

        var analysisResponse = await _client.AnalyzeSentimentAsync(text, language);
        return ConvertAnalysisResponse(analysisResponse);
    }

    private static TextAnalyticsResponse ConvertAnalysisResponse(Response<DocumentSentiment> analysisResponse)
    {
        if (analysisResponse is null)
        {
            return TextAnalyticsResponse.Neutral;
        }

        return analysisResponse.Value.Sentiment switch
        {
            TextSentiment.Positive => TextAnalyticsResponse.Positive,
            TextSentiment.Negative => TextAnalyticsResponse.Negative,
            _ => TextAnalyticsResponse.Neutral
        };
    }
}
