using Microsoft.Extensions.Options;
using ProposalCollector.Data.Configuration;
using ProposalCollector.Data.Models;
using System.Data;
using System.Data.SqlClient;

namespace ProposalCollector.Data;

public class ProposalStore : IProposalStore
{
    private readonly ProposalStoreConfiguration _configuration;

    public ProposalStore(IOptions<ProposalStoreConfiguration> options)
    {
        _configuration = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task SubmitNewProposal(string authorNickname, string title, string description)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("value cannot be empty", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("value cannot be empty", nameof(description));
        }

        var proposal = new Proposal
        {
            Id = Guid.NewGuid(),
            AuthorNickname = authorNickname,
            Title = title,
            Description = description,
            SubmittedAt = DateTime.UtcNow
        };

        using var connection = new SqlConnection(_configuration.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = BuildInsertQuery();
        SetQueryParametersToCommand(command, proposal);

        await command.ExecuteNonQueryAsync();
    }

    private void SetQueryParametersToCommand(SqlCommand command, Proposal proposal)
    {
        var id = new SqlParameter("@Id", proposal.Id);
        command.Parameters.Add(id);

        var authorNickname = new SqlParameter("@AuthorNickname", proposal.AuthorNickname);
        command.Parameters.Add(authorNickname);

        var title = new SqlParameter("@Title", proposal.Title);
        command.Parameters.Add(title);

        var description = new SqlParameter("@Description", proposal.Description);
        command.Parameters.Add(description);

        var submittedAt = new SqlParameter("@SubmittedAt", SqlDbType.DateTime2);
        submittedAt.Value = proposal.SubmittedAt;
        command.Parameters.Add(submittedAt);
    }

    private string BuildInsertQuery()
        => "INSERT INTO KITT_Proposals(Id, AuthorNickname, Title, Description, SubmittedAt) VALUES(@Id, @AuthorNickname, @Title, @Description, @SubmittedAt)";
}
