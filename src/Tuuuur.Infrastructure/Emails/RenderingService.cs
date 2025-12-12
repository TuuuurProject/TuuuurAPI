using Razor.Templating.Core;
using Tuuuur.Domain.Interfaces.Emails;

namespace Tuuuur.Infrastructure.Emails;

internal class RenderingService : IRenderingService
{
    public async Task<string> RenderAsync<T>(T p_Model) where T : class, IRenderModel
    {
        return await RazorTemplateEngine.RenderAsync($"Emails/{p_Model.GetType().Name}.cshtml", p_Model);
    }
}
