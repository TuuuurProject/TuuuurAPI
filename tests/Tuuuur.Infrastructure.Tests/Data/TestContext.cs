using Tuuuur.Infrastructure.Data.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Tuuuur.Infrastructure.Tests.Data;

public class TestContext(DbContextOptions<TestContext> p_Options) : BaseTuuuurContext(p_Options)
{
}