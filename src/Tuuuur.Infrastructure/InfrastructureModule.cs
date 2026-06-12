using Autofac;
using Tuuuur.Domain.Configuration;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Emails;
using Tuuuur.Domain.Interfaces.Services;
using Tuuuur.Domain.Interfaces.Token;
using Tuuuur.Infrastructure.Data;
using Tuuuur.Infrastructure.Data.EntityFramework;
using Tuuuur.Infrastructure.Emails;
using Tuuuur.Infrastructure.Jwt;
using Tuuuur.Infrastructure.Services;

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
        p_Builder.RegisterType<UnitOfWork<TuuuurContext>>()
                 .As<IUnitOfWork>()
                 .InstancePerLifetimeScope();

        p_Builder.RegisterType<JwtFactory>()
            .As<IJwtFactory>()
            .InstancePerLifetimeScope();

        p_Builder.RegisterConfiguration<SmtpEmailConfiguration>();

        p_Builder.RegisterType<EmailService>()
            .As<IEmailService>()
            .InstancePerLifetimeScope();

        p_Builder.RegisterType<RenderingService>()
            .As<IRenderingService>()
            .InstancePerLifetimeScope();

        p_Builder.RegisterType<CalculService>()
            .As<ICalculService>()
            .InstancePerLifetimeScope();

        p_Builder.RegisterConfiguration<CalculConfiguration>();

        p_Builder.RegisterType<EloService>()
            .As<IEloService>()
            .InstancePerLifetimeScope();

        p_Builder.RegisterConfiguration<EloConfiguration>();

        p_Builder.RegisterType<RankService>()
            .As<IRankService>()
            .InstancePerLifetimeScope();
    }
}

