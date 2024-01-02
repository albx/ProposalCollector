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
            var validationResponse = req.CreateResponse();
            if (!string.IsNullOrWhiteSpace(validationResult.ErrorMessage))
            {
                await validationResponse.WriteAsJsonAsync(
                    new ErrorResponse(validationResult.ErrorMessage),
                    HttpStatusCode.BadRequest);
            }

            return validationResponse;
        }

        _logger.LogInformation("Submitting proposal with title {Title}, {Description}", model!.Title, model.Description);

        var captchaVerificationResponse = await _reCaptchaService.VerifyAsync(
            model.CaptchaResponse,
            ProposalModel.ReCaptchaAction);

        if (!captchaVerificationResponse.Success)
        {
            var reCaptchaValidationResponse = req.CreateResponse();
            await reCaptchaValidationResponse.WriteAsJsonAsync(
                new ErrorResponse(captchaVerificationResponse.ErrorCodes.First()),
                HttpStatusCode.BadRequest);

            return reCaptchaValidationResponse;
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
