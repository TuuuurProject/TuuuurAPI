using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Tools;
using Tuuuur.Core.Responses;
using Tuuuur.Core.UseCases.Authentication;

namespace Tuuuur.Core.Tests.UseCases.Authentication
{
    public class HashUseCaseTests
    {
        private readonly HashUseCase m_HashUseCase;

        public HashUseCaseTests()
        {
            m_HashUseCase = new HashUseCase(Mock.Of<ILogger<HashUseCase>>());
        }

        [Fact]
        public async Task Handle_WhenValidRequest_ShouldReturnHashedValueAsync()
        {
            // Arrange
            HashRequest v_Request = new HashRequest("test123");

            // Act
            StringResponse v_Result = await m_HashUseCase.Handle(v_Request, default);

            // Assert
            Assert.NotNull(v_Result);
            Assert.NotEmpty(v_Result.Value);
            Assert.True(v_Result.Success);
        }
    }
}