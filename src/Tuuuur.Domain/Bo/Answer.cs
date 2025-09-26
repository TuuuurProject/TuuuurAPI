namespace Tuuuur.Domain.Bo;

public class Answer : IBOEntity
{
    public int Id { get; set; }

    public int IdQuestion { get; set; }

    public string Value { get; set; }

    public bool Valid { get; set; }

    public virtual Question Question { get; set; }
}
