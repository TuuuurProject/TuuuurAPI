using System.Text.Json.Serialization;

namespace Tuuuur.Domain.Bo;

public record History : PartyBase
{
    public int NbQuestions => PartyQuestions.Count;

    [JsonIgnore]
    public new List<PartyQuestion> PartyQuestions { get; set; } = [];
}