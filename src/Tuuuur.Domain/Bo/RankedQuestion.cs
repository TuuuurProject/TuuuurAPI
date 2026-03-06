namespace Tuuuur.Domain.Bo;

public class RankedQuestion
{
    public Question Question { get; set; }
    public int CurrentIndex { get; set; }
    public int Score { get; set; }
    public double Multiplier { get; set; }
}