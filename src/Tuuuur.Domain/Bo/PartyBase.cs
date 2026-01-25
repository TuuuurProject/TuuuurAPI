using System.Text.Json.Serialization;

namespace Tuuuur.Domain.Bo;

public record PartyBase : IBOEntity
{
    public Guid Id { get; set; }
    public DateTime Dt { get; set; }
    [JsonIgnore]
    public int IdPartyType { get; set; }
    [JsonIgnore]
    public int IdUserHost { get; set; }
    public bool Finish { get; set; }
    public int NbQuestions { get; set; }
    public double Percent { get; set; }
    public int Score => Questions.Sum(p_Question => p_Question?.Score ?? 0);
    public int Time => (int)TimeSpan.FromTicks(Questions.Sum(p_P => p_P.Ticks)).TotalMilliseconds;
    public User UserHost { get; set; }
    public PartyType PartyType { get; set; }
    public List<User> Users { get; set; }
    public List<Difficulty> Difficulties { get; set; }
    public List<Theme>  Themes { get; set; }
    public List<Question> Questions { get; set; }
}