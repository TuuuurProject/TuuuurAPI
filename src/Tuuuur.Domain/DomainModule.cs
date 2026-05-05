using System.Diagnostics.CodeAnalysis;
using Autofac;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Emails;
using Tuuuur.Domain.Interfaces.Services;
using Tuuuur.Domain.Interfaces.Token;

namespace Tuuuur.Domain;

public class DomainModule : Module
{
    [SuppressMessage("Style", "IDE1006:Styles d'affectation de noms", Justification = "Inherited named")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);
        CustomLoad(builder);
    }

    /// <summary>
    /// Custom load for all custom DI
    /// </summary>
    /// <param name="p_Builder"></param>
    private static void CustomLoad(ContainerBuilder p_Builder)
    {
        
    }
}

