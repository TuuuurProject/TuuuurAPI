using Microsoft.EntityFrameworkCore;

namespace Tuuuur.Infrastructure.Data.EntityFramework;

public class TuuuurContext : BaseTuuuurContext
{
    public const string ConnectionStringName = "Tuuuur";

    public TuuuurContext(DbContextOptions<TuuuurContext> options)
        : base(options)
    {
    }
}