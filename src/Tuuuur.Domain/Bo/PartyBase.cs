namespace Tuuuur.Domain.Bo;

public record PartyBase : IBOEntity
{
    public Guid Id { get; set; }
    public DateTime Dt { get; set; }
    public int IdPartyType { get; set; }
    public int IdUserHost { get; set; }
    public bool Active { get; set; }
    public bool Finish { get; set; }
    public int NbQuestions { get; set; }
    public double Percent => Questions.Count != 0 
        ? ((double)Questions.Count(p_Question => p_Question.Correct) / NbQuestions * 100) 
        : 0;
    public int Score => Questions.Sum(p_Question => p_Question.Score);
    public int Time => (int)TimeSpan.FromTicks(Questions.Sum(p_P => p_P.Ticks)).TotalSeconds;
    public PartyType Type { get; set; }
    public List<User> Users { get; set; }
    public List<Difficulty> Difficulties { get; set; }
    public List<Theme>  Themes { get; set; }
    public List<Question> Questions { get; set; }
}