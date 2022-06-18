using ProposalCollector.Api.Models;

namespace ProposalCollector.Api.Services;

public interface ITextAnalyticsService
{
    Task<TextAnalyticsResponse> AnalyzeAsync(string text);
}
