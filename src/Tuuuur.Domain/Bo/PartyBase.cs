using System.Linq;
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
    public int Score => Questions?.Sum(p_Question => p_Question?.Score ?? 0) ?? 0;

    public int Time
    {
        get
        {
            long v_Ticks = Questions?.Where(p_Question => p_Question != null).Sum(p_Question => p_Question.Ticks) ?? 0L;
            return (int)TimeSpan.FromTicks(v_Ticks).TotalMilliseconds;
        }
    }
    public User UserHost { get; set; }
    public PartyType PartyType { get; set; }
    public List<User> Users { get; set; }
    public List<Difficulty> Difficulties { get; set; }
    public List<Theme> Themes { get; set; }
    public List<Question> Questions { get; set; }
}