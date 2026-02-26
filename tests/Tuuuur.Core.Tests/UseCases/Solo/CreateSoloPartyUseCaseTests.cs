using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Responses;
using Tuuuur.Core.UseCases.Parties;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Domain.Security;
using Tuuuur.Factory.Tests;

namespace Tuuuur.Core.Tests.UseCases.Parties;

public class CreateSoloPartyUseCaseTests
{
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<ILogger<CreateSoloPartyUseCase>> m_LoggerMock;
    private readonly Mock<IUserRoleService> m_UserRoleService;

    private readonly CreateSoloPartyUseCase m_UseCase;
    
    public CreateSoloPartyUseCaseTests()
    {
        m_UnitOfWorkMock = new Mock<IUnitOfWork>();
        m_LoggerMock = new Mock<ILogger<CreateSoloPartyUseCase>>();
        m_UserRoleService = new Mock<IUserRoleService>();

        m_UseCase = new CreateSoloPartyUseCase(m_UnitOfWorkMock.Object, m_LoggerMock.Object, m_UserRoleService.Object);
    }
    
    [Fact]
    public async Task Handle_ExpectedAsync()
    {
        // Arrange
        User v_User = BoFactory.CreateUser().Generate();
        Party v_Party = BoFactory.CreateParty().Generate();
        m_UserRoleService.Setup(p_P => p_P.GetEmail()).Returns(v_User.Email);
        m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(v_User);
        
        m_UnitOfWorkMock.Setup(p_U => p_U.QuestionRepository.GetQuestionsByThemesIdsAndDifficultiesIdsAndNumberOfQuestionsAsync
            (It.IsAny<int[]>(),It.IsAny<int[]>(),It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<Question>());

        Mock<IMappingAddEntity<Party, IEntity>> v_MappingAddEntityMock = new();
        v_MappingAddEntityMock.Setup(p_C => p_C.MapBoEntity).Returns(v_Party);
        
        m_UnitOfWorkMock.Setup(p_U => p_U.PartyRepository.CreatePartyAsync
            (It.IsAny<Party>(), It.IsAny<CancellationToken>())).ReturnsAsync(v_MappingAddEntityMock.Object);
        
        CreateSoloPartyRequest v_Request = new([1, 2, 3],[1, ],30);

        // Act
        GuidResponse v_Result = await m_UseCase.Handle(v_Request, CancellationToken.None);

        // Assert
        Assert.NotNull(v_Result);
        Assert.True(v_Result.Success);

        // Assert
        m_UnitOfWorkMock.Verify(p_Uow => p_Uow.UserRepository.GetUserByEmailAsync(v_User.Email, It.IsAny<CancellationToken>()), Times.Once);
        v_Result.Success.Should().BeTrue();
        v_Result.Errors.Should().BeNull();
    }
}