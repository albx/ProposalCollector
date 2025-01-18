using KITT.Web.ReCaptcha.Http.v3;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ProposalCollector.Api.Services;
using ProposalCollector.Data;
using ProposalCollector.Shared;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace ProposalCollector.Api;

public class SubmitProposalFunction
{
    private readonly ILogger _logger;
    private readonly IProposalStore _proposalStore;
    private readonly ReCaptchaService _reCaptchaService;
    private readonly ITextAnalyticsService _textAnalyticsService;

    public SubmitProposalFunction(
        ILoggerFactory loggerFactory,
        IProposalStore proposalStore,
        ReCaptchaService reCaptchaService,
        ITextAnalyticsService textAnalyticsService)
    {
        _logger = loggerFactory.CreateLogger<SubmitProposalFunction>();
        _proposalStore = proposalStore ?? throw new ArgumentNullException(nameof(proposalStore));
        _reCaptchaService = reCaptchaService ?? throw new ArgumentNullException(nameof(reCaptchaService));
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
                await validationResponse.WriteAsJsonAsync(new ErrorResponse(validationResult.ErrorMessage));
            }

            return validationResponse;
        }

        _logger.LogInformation("Submitting proposal with title {Title}, {Description}", model!.Title, model.Description);

        try
        {
            await VerifyReCaptchaAsync(model);
            await AnalyzeSentimentAnalysisAsync(model);

            await _proposalStore.SubmitNewProposal(model.AuthorNickname, model.Title, model.Description);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");

            return response;
        }
        catch (ValidationException ex)
        {
            var validationErrorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await validationErrorResponse.WriteAsJsonAsync(new ErrorResponse(ex.Message));

            return validationErrorResponse;
        }
        catch (Exception ex)
        {
            var serverErrorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await serverErrorResponse.WriteAsJsonAsync(new ErrorResponse(ex.Message));

            return serverErrorResponse;
        }
    }

    private async Task VerifyReCaptchaAsync(ProposalModel model)
    {
        try
        {
            var captchaVerificationResponse = await _reCaptchaService.VerifyAsync(
                model.CaptchaResponse,
                ProposalModel.ReCaptchaAction);

            if (!captchaVerificationResponse.Success)
            {
                throw new ValidationException(captchaVerificationResponse.ErrorCodes.First());
            }
        }
        catch (InvalidOperationException ex)
        {
            //this exception could happen if the reCaptcha action does not match.
            _logger.LogError(ex, "Invalid operation during reCaptcha verification: {ErrorMessage}", ex.Message);
            throw new ValidationException(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying reCatpcha: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    private async Task AnalyzeSentimentAnalysisAsync(ProposalModel model)
    {
        try
        {
            var sentimentAnalysis = await _textAnalyticsService.AnalyzeAsync(model.Description);
            if (sentimentAnalysis == Models.TextAnalyticsResponse.Negative)
            {
                throw new ValidationException("Your text is negative");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing sentiment analysis: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    private static Models.ValidationResult ValidateModel(ProposalModel? model)
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
