using System.Text.Json.Serialization;

namespace Tuuuur.Domain.Bo;

public record History : PartyBase
{
    public int NbQuestions => PartyQuestions.Count;

    [JsonIgnore]
    public override List<PartyQuestion> PartyQuestions { get; set; }
}