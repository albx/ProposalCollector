using System.ComponentModel.DataAnnotations;

namespace ProposalCollector.Shared;

public record ProposalModel
{
    public string AuthorNickname { get; set; } = string.Empty;

    [Required(ErrorMessageResourceName = nameof(Title), ErrorMessageResourceType = typeof(Resources.ProposalModel))]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessageResourceName = nameof(Description), ErrorMessageResourceType = typeof(Resources.ProposalModel))]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string CaptchaResponse { get; set; } = string.Empty;
}
