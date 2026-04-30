using System.Text.Json.Serialization;

namespace Tuuuur.Domain.Bo;

public class PartyUser : IBOEntity
{
    public int Id { get; set; }

    public Guid IdUser { get; set; }

    public Guid IdParty { get; set; }

    public User User { get; set; }
    
    [JsonIgnore]
    public int? Elo { get; set; }
    
    [JsonIgnore]
    public bool? Winner { get; set; }
    
    [JsonIgnore]
    public int? FinalScore { get; set; }
}
