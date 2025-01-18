using KITT.Web.ReCaptcha.Http.v3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest request)
    {
        var model = await request.ReadFromJsonAsync<ProposalModel>();
        var validationResult = ValidateModel(model);
        if (!validationResult.IsValid)
        {
            var errorMessage = "Invalid data";
            if (!string.IsNullOrWhiteSpace(validationResult.ErrorMessage))
            {
                errorMessage = validationResult.ErrorMessage;
            }

            return new BadRequestObjectResult(new ErrorResponse(errorMessage));
        }

        _logger.LogInformation("Submitting proposal with title {Title}, {Description}", model!.Title, model.Description);

        try
        {
            await VerifyReCaptchaAsync(model);
            await AnalyzeSentimentAnalysisAsync(model);

            await _proposalStore.SubmitNewProposal(model.AuthorNickname, model.Title, model.Description);

            return new OkResult();
        }
        catch (ValidationException ex)
        {
            return new BadRequestObjectResult(new ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return new ObjectResult(new ErrorResponse(ex.Message))
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
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
