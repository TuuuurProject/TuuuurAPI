using Autofac;
using System.Reflection;
using Tuuuur.API.Presenters;
using Tuuuur.API.Notifications;
using Tuuuur.API.Security;
using Tuuuur.API.Hubs;
using Tuuuur.Domain.Notifications;
using Tuuuur.Domain.Security;
using Tuuuur.Domain.Hubs;
using Tuuuur.API.Configuration;
using Tuuuur.Domain.Configuration;
using Module = Autofac.Module;

namespace Tuuuur.API;

/// <summary>
/// Module for API
/// </summary>
internal class ApiModule : Module
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
        // Add here your services
        p_Builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .Where(t => t.Name.EndsWith("Presenter"))
            .InstancePerLifetimeScope();

        p_Builder.RegisterGeneric(typeof(GenericEntityListPresenter<>))
            .AsSelf()
            .InstancePerLifetimeScope();

        p_Builder.RegisterGeneric(typeof(GenericEntityPresenter<>))
            .AsSelf();

        p_Builder.RegisterType<NotificationsService>()
            .As<INotificationsService>()
            .InstancePerLifetimeScope();

        p_Builder.RegisterType<GroupNotificationService>()
            .As<IGroupNotificationService>()
            .InstancePerLifetimeScope();

        p_Builder.RegisterType<UserRoleService>()
            .As<IUserRoleService>()
            .InstancePerLifetimeScope();

        p_Builder.RegisterConfiguration<GoogleConfiguration>();
    }
}