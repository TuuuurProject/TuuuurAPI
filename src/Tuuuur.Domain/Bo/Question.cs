using System.Text.Json.Serialization;

namespace Tuuuur.Domain.Bo;

public class Question : IBOEntity
{
    public int Id { get; set; }
    public string Label { get; set; }
    public List<Answer> Answers { get; set; } = [];
    public Difficulty Difficulty { get; set; }
    public List<Theme> Themes { get; set; } = [];
    public int Index { get; set; }
    public int Score { get; set; }
    public int? IdUserAnswer { get; set; }
    public bool Correct { get; set; }
    public DateTime? DtPresentedAt  { get; set; }
    public DateTime? DtAnsweredAt  { get; set; }
    [JsonIgnore]
    public int Ticks { get; init; }
    public int Time => (int)TimeSpan.FromTicks(Ticks).TotalMilliseconds;
    [JsonIgnore]
    public Guid AnswerSeed { get; set; }
    
    public void ClearAnswer()
    {
        foreach (Answer v_Answer in Answers)
        {
            v_Answer.Valid = null;
        }
    }

    public Question Copy()
    {
        return new Question
        {
            Id = Id,
            Label = Label,
            Answers = [.. Answers],
            Difficulty = Difficulty,
            Themes = Themes
        };
    }
}