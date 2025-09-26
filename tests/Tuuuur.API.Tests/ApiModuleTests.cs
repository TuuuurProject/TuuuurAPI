using Autofac;
using Autofac.Core;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using Tuuuur.API.Notifications;
using Tuuuur.API.Presenters;
using Tuuuur.API.Presenters.Authentication;
using Tuuuur.API.Security;
using Tuuuur.Domain.Notifications;
using Tuuuur.Domain.Security;

namespace Tuuuur.API.Tests
{
    public class ApiModuleTests
    {
        private static readonly string m_AutofacClass = $"{nameof(Autofac)}.";
        private readonly IList<Type> m_TypesWanted;
        private IList<Type> m_TypesLoaded;

        public ApiModuleTests()
        {
            m_TypesWanted = new List<Type>
            {
                typeof(JwtAuthenticationPresenter),
                typeof(ValidationPresenter),
                typeof(EmptyPresenter),
                typeof(GuidPresenter),
                typeof(INotificationsService),
                typeof(IUserRoleService)
            };
        }

        [Fact]
        public void Should_Have_Register_Types()
        {
            ContainerBuilder v_ContainerBuilder = new();
            v_ContainerBuilder.RegisterModule(new ApiModule());

            IContainer v_Container = v_ContainerBuilder.Build();

            m_TypesLoaded = v_Container.ComponentRegistry.Registrations
                                       .SelectMany(p_X => p_X.Services)
                                       .Cast<IServiceWithType>()
                                       .Select(p_X => p_X.ServiceType)
                                       // Exclude types registered by AutoFac or IBaseRequest from MediatR
                                       .Where(p_C =>
                                           !p_C.FullName.StartsWith(m_AutofacClass) &&
                                           p_C.Name != nameof(IBaseRequest))
                                       .Distinct()
                                       .ToList();

            Check.That(m_TypesLoaded.OrderBy(c => c.FullName)).ContainsExactly(m_TypesWanted.OrderBy(c => c.FullName));
        }
    }
}