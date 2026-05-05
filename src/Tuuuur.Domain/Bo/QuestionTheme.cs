namespace Tuuuur.Domain.Bo;

public class QuestionTheme : IBOEntity
{
    public int Id { get; set; }

    public int IdTheme { get; set; }

    public virtual Theme Theme { get; set; }
}
