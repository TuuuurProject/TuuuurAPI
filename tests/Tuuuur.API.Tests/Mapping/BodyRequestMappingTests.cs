using AutoMapper;
using Tuuuur.API.Requests.Mapping;

namespace Tuuuur.API.Tests.Mapping
{
    public class BodyRequestMappingTests
    {
        public static MapperConfiguration InitializeAutoMapper()
        {
            return new MapperConfiguration(p_Cfg =>
                p_Cfg.AddProfile(new BodyRequestProfile())
            );  //mapping between EF Core Entity and Business layer objects
        }

        [Fact]
        public void ValidateProfile()
        {
            Check.ThatCode(() =>
                new MapperConfiguration(cfg =>
                        cfg.AddProfile(typeof(BodyRequestProfile)))
                    .AssertConfigurationIsValid()
            ).DoesNotThrow();
        }
    }
}