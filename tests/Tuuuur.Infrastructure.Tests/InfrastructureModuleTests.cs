using Autofac;
using Autofac.Core;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Emails;
using Tuuuur.Domain.Interfaces.Token;
using Tuuuur.Infrastructure.Emails;

namespace Tuuuur.Infrastructure.Tests
{
    public class InfrastructureModuleTests
    {
        private static readonly string m_AutofacClass = $"{nameof(Autofac)}.";
        private readonly IList<Type> m_TypesWanted;
        private IList<Type> m_TypesLoaded;

        public InfrastructureModuleTests()
        {
            m_TypesWanted = new List<Type>
            {
                typeof(IUnitOfWork),
                typeof(IJwtFactory),
                typeof(IEmailService),
                typeof(IRenderingService),
                typeof(SmtpEmailConfiguration)
            };
        }

        [Fact]
        public void Should_Have_Register_Types()
        {
            ContainerBuilder v_ContainerBuilder = new();
            v_ContainerBuilder.RegisterModule(new InfrastructureModule());

            IContainer v_Container = v_ContainerBuilder.Build();

            m_TypesLoaded = v_Container.ComponentRegistry.Registrations
                                       .SelectMany(p_X => p_X.Services)
                                       .Cast<IServiceWithType>()
                                       .Select(p_X => p_X.ServiceType)
                                       // Exclude types registered by AutoFac
                                       .Where(p_C => !p_C.FullName.StartsWith(m_AutofacClass))
                                       .Distinct()
                                       .ToList();

            Check.That(m_TypesLoaded.OrderBy(p_C => p_C.FullName)).ContainsExactly(m_TypesWanted.OrderBy(p_C => p_C.FullName));
        }
    }
}