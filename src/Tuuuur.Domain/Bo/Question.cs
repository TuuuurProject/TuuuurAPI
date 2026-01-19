using System.Text.Json.Serialization;

namespace Tuuuur.Domain.Bo;

public class Question : IBOEntity
{
    public int Id { get; set; }
    public string Label { get; set; }
    public int IdDifficulty { get; set; }
    public List<Answer> Answer { get; set; } = [];
    public Difficulty Difficulty { get; set; }
    public List<Theme> Themes { get; set; } = [];
    public int Index { get; set; }
    public int Score { get; set; }
    public int? UserAnswer { get; set; }
    public bool Correct { get; set; }
    public DateTime? DtPresentedAt  { get; set; }
    [JsonIgnore]
    public DateTime? DtAnsweredAt  { get; set; }
    [JsonIgnore]
    public int Ticks { get; set; }
    [JsonIgnore]
    public Guid AnswerSeed { get; set; }
    
    public void ClearAnswer()
    {
        foreach (Answer v_Answer in Answer)
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
            IdDifficulty = IdDifficulty,
            Answer = [.. Answer],
            Difficulty = Difficulty,
            Themes = Themes
        };
    }
}