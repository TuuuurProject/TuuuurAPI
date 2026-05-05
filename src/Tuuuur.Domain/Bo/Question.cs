namespace Tuuuur.Domain.Bo;

public class Question : IBOEntity
{
    public int Id { get; set; }
    public string Label { get; set; }

    public int IdDifficulty { get; set; }

    public virtual List<Answer> Answer { get; set; } = [];
    public virtual Difficulty Difficulty { get; set; }

    public virtual List<PartyQuestion> PartyQuestion { get; set; } = [];

    public virtual List<QuestionTheme> QuestionTheme { get; set; } = [];

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
            PartyQuestion = PartyQuestion,
            QuestionTheme = QuestionTheme
        };
    }
}