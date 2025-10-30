using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Tools;
using Tuuuur.Core.Responses;
using Tuuuur.Core.UseCases.Authentication;
using Tuuuur.Domain.Interfaces.Data;

namespace Tuuuur.Core.Tests.UseCases.Authentication;

public class HashUseCaseTests
{
    private readonly HashUseCase m_UseCase;

    public HashUseCaseTests()
    {
        Mock<IUnitOfWork> v_UnitOfWorkMock = new();
        Mock<ILogger<HashUseCase>> v_LoggerMock = new();
        m_UseCase = new HashUseCase(v_LoggerMock.Object, v_UnitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldReturnHashedValueAsync()
    {
        // Arrange
        HashRequest v_Request = new HashRequest("test123");

        // Act
        StringResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        Assert.NotNull(v_Result);
        Assert.NotEmpty(v_Result.Value);
        Assert.True(v_Result.Success);
    }
}
