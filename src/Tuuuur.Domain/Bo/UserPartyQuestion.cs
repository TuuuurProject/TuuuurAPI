namespace Tuuuur.Domain.Bo;

public class UserPartyQuestion : IBOEntity
{
    public int Id { get; set; }

    public int IdPartyQuestion { get; set; }

    public int IdUser { get; set; }

    public DateTime DtPresentedAt { get; set; }

    public DateTime? DtAnsweredAt { get; set; }
    
    public int? IdAnwser { get; set; }

    public bool? Correct { get; set; }
    
    public int Score { get; set; }
    
    public virtual Answer Anwser { get; set; }

    public virtual PartyQuestion PartyQuestion { get; set; }

    public virtual User User { get; set; }
    
    public void CalculateScore(int p_MaxPoints, double p_MaxDurationInSeconds)
    {
        if (DtAnsweredAt is null)
            throw new NotImplementedException();
        
        double v_ResponseDuration = (DtAnsweredAt - DtPresentedAt).Value.TotalSeconds;

        if (v_ResponseDuration >= p_MaxDurationInSeconds)
        {
            Score = 0;
        }
        else if (v_ResponseDuration <= 0)
        {
            Score = p_MaxPoints;
        }
        else
        {
            double v_Score = p_MaxPoints * (1 - (v_ResponseDuration / p_MaxDurationInSeconds));

            Score = (int)Math.Round(v_Score);
        }
    }
}
