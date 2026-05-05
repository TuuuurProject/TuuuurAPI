namespace Tuuuur.Domain.Interfaces.Emails;

public interface IRenderingService
{
    Task<string> RenderAsync<T>(T p_Model) where T : class, IRenderModel;
}