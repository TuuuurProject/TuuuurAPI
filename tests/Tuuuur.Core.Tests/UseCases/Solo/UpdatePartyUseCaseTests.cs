using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Requests.Parties;
using Tuuuur.Core.Responses;
using Tuuuur.Core.UseCases.Parties;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Bo.Enum;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Domain.Interfaces.Services;
using Tuuuur.Domain.Security;
using Tuuuur.Factory.Tests;

namespace Tuuuur.Core.Tests.UseCases.Parties;

public class UpdatePartyUseCaseTests
{
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock;
    private readonly Mock<ILogger<UpdatePartyUseCase>> m_LoggerMock;
    private readonly Mock<IUserRoleService> m_UserRoleService;
    private readonly Mock<ICalculService> m_CalculService;

    private readonly UpdatePartyUseCase m_UseCase;
    
    public UpdatePartyUseCaseTests()
    {
        m_UnitOfWorkMock = new Mock<IUnitOfWork>();
        m_LoggerMock = new Mock<ILogger<UpdatePartyUseCase>>();
        m_UserRoleService = new Mock<IUserRoleService>();
        m_CalculService = new Mock<ICalculService>();

        m_UseCase = new UpdatePartyUseCase(m_UnitOfWorkMock.Object, m_LoggerMock.Object, m_CalculService.Object, m_UserRoleService.Object);
    }
    
    [Fact]
    public async Task Handle_ExpectedAsync()
    {
        // Arrange
        User v_User = BoFactory.CreateUser().Generate();
        Party v_Party = BoFactory.CreateParty().Generate();
        int v_AnswerId = 1;
        v_Party.IdPartyType = (int)PartyTypeType.Solo;
        v_Party.PartyUsers = [new PartyUser { User = v_User }];
        v_Party.PartyQuestions =
        [
            new PartyQuestion()
            {
                UserPartyQuestion = new UserPartyQuestion()
                {
                    Correct = null,
                    DtAnsweredAt = DateTime.Now,
                    DtPresentedAt = DateTime.Now.AddSeconds(3),
                },
                Order = 1,
                Question = new Question()
                {
                    Answers =
                    [
                        new Answer()
                        {
                            Id = v_AnswerId,
                            Valid = true
                        }
                    ]
                }
                
            }
        ];
        
        m_UserRoleService.Setup(p_P => p_P.GetCurrentUserEmail()).Returns(v_User.Email);
        m_CalculService.Setup(p_U => p_U.CalculateScore(It.IsAny<DateTime>(), It.IsAny<DateTime?>())).Returns(753);

        
        m_UnitOfWorkMock.Setup(p_U => p_U.UserRepository.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(v_User);
        m_UnitOfWorkMock.Setup(p_U => p_U.PartyRepository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(v_Party);
        m_UnitOfWorkMock.Setup(p_U => p_U.PartyRepository.FinishPartyAsync(It.IsAny<Party>())).Returns(Task.CompletedTask);
        m_UnitOfWorkMock.Setup(p_U => p_U.UserPartyQuestionRepository.UpdateAsync(It.IsAny<UserPartyQuestion>())).Returns(Task.CompletedTask);
        
        UpdateSoloPartyStateRequest v_Request = new(Guid.Empty, 1);

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
