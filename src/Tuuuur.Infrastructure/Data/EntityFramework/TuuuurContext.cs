using Microsoft.EntityFrameworkCore;

namespace Tuuuur.Infrastructure.Data.EntityFramework;

public class TuuuurContext(DbContextOptions<TuuuurContext> p_DbContextOptions) 
    : BaseTuuuurContext(p_DbContextOptions)
{
    public const string ConnectionStringName = "Tuuuur";
}