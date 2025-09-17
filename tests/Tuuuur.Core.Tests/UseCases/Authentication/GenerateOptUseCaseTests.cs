using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Responses.Authentication;
using Tuuuur.Core.UseCases.Authentication;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Data.Entities;

namespace Tuuuur.Core.Tests.UseCases.Authentication;

public class GenerateOptUseCaseTests
{
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<ILogger<GenerateOptUseCase>> m_LoggerMock;

    private readonly GenerateOptUseCase m_UseCase;
    
    public GenerateOptUseCaseTests()
    {
        m_UnitOfWorkMock = new Mock<IUnitOfWork>();
        m_LoggerMock = new Mock<ILogger<GenerateOptUseCase>>();

        m_UseCase = new GenerateOptUseCase(m_UnitOfWorkMock.Object, m_LoggerMock.Object);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnUserAuthAsync()
    {
        // Arrange
        User v_User = new User { Id = 1 };
        UserAuth v_UserAuth = new UserAuth { User = v_User, UserId = v_User.Id, Code = "123456" };
        Mock<IMappingAddEntity<UserAuth, IEntity>> v_MappingAddEntityMock = new();
        v_MappingAddEntityMock.Setup(p_C => p_C.MapBoEntity).Returns(v_UserAuth);

        m_UnitOfWorkMock.Setup(p_Uow => p_Uow.UserAuthRepository.AddAuthCodeAsync(It.IsAny<UserAuth>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(v_MappingAddEntityMock.Object);

        GenerateOptRequest v_Request = new(v_User);

        // Act
        UserAuthResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        Assert.NotNull(v_Result);
        Assert.True(v_Result.Success);
        Assert.Equal(v_User, v_Result.Value.User);
    }
}
