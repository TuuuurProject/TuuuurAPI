using Autofac;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Token;
using Tuuuur.Infrastructure.Data;
using Tuuuur.Infrastructure.Data.EntityFramework;
using Tuuuur.Infrastructure.Jwt;

namespace Tuuuur.Infrastructure;

public class InfrastructureModule : Module
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
/*        p_Builder.RegisterType<UnitOfWork<TuuuurContext>>()
                 .As<IUnitOfWork>()
                 .InstancePerLifetimeScope();*/
        p_Builder.RegisterType<JwtFactory>()
                 .As<IJwtFactory>()
                 .InstancePerLifetimeScope();
    }
}