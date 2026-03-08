using Tuuuur.Domain.Interfaces.Services;

namespace Tuuuur.Infrastructure.Services;

public class CalculService(CalculConfiguration p_CalculConfiguration) : ICalculService
{
    public int CalculateScore(DateTime p_DtPresentedAt, DateTime? p_DtAnsweredAt)
    {
        if (p_DtAnsweredAt is null)
            return 0;

        double v_ResponseDuration = (p_DtAnsweredAt - p_DtPresentedAt).Value.TotalSeconds;

        if (v_ResponseDuration >= p_CalculConfiguration.MaxDurationInSeconds)
        {
            return 0;
        }
        else if (v_ResponseDuration <= 0)
        {
            return p_CalculConfiguration.MaxScore;
        }
        else
        {
            double v_Score = p_CalculConfiguration.MaxScore * (1 - (v_ResponseDuration / p_CalculConfiguration.MaxDurationInSeconds));

            return (int)Math.Round(v_Score);
        }
    }
}
