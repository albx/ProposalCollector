using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ProposalCollector.Api.Services;
using ProposalCollector.Data;
using ProposalCollector.Shared;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace ProposalCollector.Api;

public partial class SubmitProposalFunction
{
    private readonly ILogger _logger;
    private readonly IProposalStore _proposalStore;
    private readonly CaptchaService _captchaService;
    private readonly ITextAnalyticsService _textAnalyticsService;

    public SubmitProposalFunction(
        ILoggerFactory loggerFactory,
        IProposalStore proposalStore,
        CaptchaService captchaService,
        ITextAnalyticsService textAnalyticsService)
    {
        _logger = loggerFactory.CreateLogger<SubmitProposalFunction>();
        _proposalStore = proposalStore ?? throw new ArgumentNullException(nameof(proposalStore));
        _captchaService = captchaService ?? throw new ArgumentNullException(nameof(captchaService));
        _textAnalyticsService = textAnalyticsService ?? throw new ArgumentNullException(nameof(textAnalyticsService));
    }

    [Function("SubmitProposal")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        var model = await req.ReadFromJsonAsync<ProposalModel>();
        var validationResult = ValidateModel(model);
        if (!validationResult.IsValid)
        {
            var validationResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            if (!string.IsNullOrWhiteSpace(validationResult.ErrorMessage))
            {
                await validationResponse.WriteAsJsonAsync(
                    new ErrorResponse(validationResult.ErrorMessage),
                    HttpStatusCode.BadRequest);
            }

            return validationResponse;
        }

        _logger.LogInformation("Submitting proposal with title {Title}, {Description}", model.Title, model.Description);

        var captchaVerificationResponse = await _captchaService.VerifyAsync(model.CaptchaResponse);
        if (!captchaVerificationResponse.Success)
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var sentimentAnalysis = await _textAnalyticsService.AnalyzeAsync(model.Description);
        if (sentimentAnalysis == Models.TextAnalyticsResponse.Negative)
        {
            var sentimentAnalysisResponse = req.CreateResponse();
            await sentimentAnalysisResponse.WriteAsJsonAsync(
                new ErrorResponse("Your text is negative"),
                HttpStatusCode.BadRequest);

            return sentimentAnalysisResponse;
        }

        await _proposalStore.SubmitNewProposal(model.AuthorNickname, model.Title, model.Description);

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");

        return response;
    }

    private Models.ValidationResult ValidateModel(ProposalModel? model)
    {
        if (model is null)
        {
            return new Models.ValidationResult(false, null);
        }

        try
        {
            var context = new ValidationContext(model);
            Validator.ValidateObject(model, context, true);

            return Models.ValidationResult.Success;
        }
        catch (ValidationException ex)
        {
            return new Models.ValidationResult(false, ex.Message);
        }
    }
}
