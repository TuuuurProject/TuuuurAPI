using AutoMapper;

namespace Tuuuur.Infrastructure.Data.Mapping;

internal class AMappingClass
{
    protected AMappingClass(IMapper p_Mapper)
    {
        Mapper = p_Mapper ?? throw new ArgumentNullException(nameof(p_Mapper), "Mapper cannot be null");
    }

    public IMapper Mapper { get; }
}