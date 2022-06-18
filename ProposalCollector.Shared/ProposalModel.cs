using System.ComponentModel.DataAnnotations;

namespace ProposalCollector.Shared;

public record ProposalModel
{
    public string AuthorNickname { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string CaptchaResponse { get; set; } = string.Empty;
}
