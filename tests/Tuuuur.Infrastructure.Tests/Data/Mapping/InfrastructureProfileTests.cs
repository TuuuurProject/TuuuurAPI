using AutoMapper;
using Tuuuur.Infrastructure.Data.Mapping;

namespace Tuuuur.Infrastructure.Tests.Data.Mapping
{
    public class InfrastructureProfileTests
    {
        public static MapperConfiguration InitializeAutoMapper()
        {
            return new MapperConfiguration(p_Cfg =>
                p_Cfg.AddProfile(new InfrastructureProfile())
            );  //mapping between EF Core Entity and Business layer objects
        }

        [Fact]
        public void ValidateProfile()
        {
            Check.ThatCode(() =>
                new MapperConfiguration(p_Cfg =>
                        p_Cfg.AddProfile(typeof(InfrastructureProfile)))
                    .AssertConfigurationIsValid()
            ).DoesNotThrow();
        }
    }
}