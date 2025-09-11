using Tuuuur.Infrastructure.Tools;

namespace Tuuuur.Infrastructure.Tests.Tools
{
    public class ReflectionToolsTests
    {
        [Fact]
        public void GetDefaultValue_StateUnderTest_ExpectedBehavior()
        {
            Check.That(typeof(int).GetDefaultValue()).IsEqualTo(default(int));
            Check.That(typeof(string).GetDefaultValue()).IsEqualTo(default(string));
            Check.That(typeof(ReflectionToolsTests).GetDefaultValue()).IsEqualTo(default(ReflectionToolsTests));
        }
    }
}