using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests.Parties;
using Tuuuur.Core.Responses;
using Tuuuur.Core.UseCases.Parties;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Bo.Enum;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Security;
using Tuuuur.Factory.Tests;

namespace Tuuuur.Core.Tests.UseCases.Solo;

public class GetSoloUseCaseTests
{
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<ILogger<GetSoloUseCase>> m_LoggerMock;
    private readonly Mock<IUserRoleService> m_UserRoleService;

    private readonly GetSoloUseCase m_UseCase;
    
    public GetSoloUseCaseTests()
    {
        m_UnitOfWorkMock = new Mock<IUnitOfWork>();
        m_LoggerMock = new Mock<ILogger<GetSoloUseCase>>();
        m_UserRoleService = new Mock<IUserRoleService>();

        m_UseCase = new GetSoloUseCase(m_UnitOfWorkMock.Object, m_LoggerMock.Object, m_UserRoleService.Object);
    }
    
    [Fact]
    public async Task Handle_ExpectedAsync()
    {
        // Arrange
        User v_User = BoFactory.CreateUser().Generate();
        Party v_Party = BoFactory.CreateParty().Generate();
        v_Party.IdPartyType = (int)PartyTypeType.Solo;
        v_Party.PartyUsers = [new () { User = v_User }];
        
        m_UserRoleService.Setup(p_P => p_P.GetEmail()).Returns(v_User.Email);
        m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(v_User);
        m_UnitOfWorkMock.Setup(p_U => p_U.PartyRepository.GetPartyByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(v_Party);
        
        GetSoloRequest v_Request = new(Guid.Empty);

        // Act
        GenericEntityResponse<Party> v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        Assert.NotNull(v_Result);
        Assert.True(v_Result.Success);

        // Assert
        m_UnitOfWorkMock.Verify(p_Uow => p_Uow.UserRepository.GetUserByEmailAsync(v_User.Email, It.IsAny<CancellationToken>()), Times.Once);
        v_Result.Success.Should().BeTrue();
        v_Result.Errors.Should().BeNull();
    }
}
