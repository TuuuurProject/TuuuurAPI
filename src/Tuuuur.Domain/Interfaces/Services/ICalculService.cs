namespace Tuuuur.Domain.Interfaces.Services;

public interface ICalculService
{
    public int CalculateScore(DateTime p_DtPresentedAt, DateTime? p_DtAnsweredAt);
}
