using MediatR;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Requests.Tools;
using Tuuuur.Core.Responses;
using Tuuuur.Core.Responses.Authentication;
using Tuuuur.Core.UseCases;
using Tuuuur.Core.UseCases.Authentication;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Errors;
using Tuuuur.Domain.Interfaces.Data;
using Tuuuur.Domain.Interfaces.Data.Entities;
using Tuuuur.Domain.Interfaces.Emails;

namespace Tuuuur.Core.Tests.UseCases.Authentication;

public class ThemeListUseCaseTests
{
    private readonly Mock<IUnitOfWork> m_UnitOfWorkMock = new();
    private readonly Mock<ILogger<ThemeListUseCase>> m_LoggerMock = new();

    private ThemeListUseCase m_UseCase;

    [Fact]
    public async Task Handle_GetAll_ShouldReturnThemeListAsync()
    {
        // Arrange
        CancellationToken v_CancellationToken = CancellationToken.None;
        
        GenericEntityListRequest<Theme> v_Request = new();

        m_UnitOfWorkMock.Setup(p_Uow => p_Uow.ThemeRepository.GetAllThemesAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new List<Theme>());

        m_UseCase = new ThemeListUseCase(m_UnitOfWorkMock.Object, m_LoggerMock.Object);

        // Act
        GenericEntityListResponse<Theme> v_Result = await m_UseCase.Handle(v_Request, v_CancellationToken);

        // Assert
        m_UnitOfWorkMock.Verify(p_Uow => p_Uow.ThemeRepository.GetAllThemesAsync(It.IsAny<CancellationToken>()), Times.Once);
        v_Result.Success.Should().BeTrue();
        v_Result.Errors.Should().BeNull();
    }
    
    [Fact]
    public async Task Handle_GetAll_ShouldReturnExceptionAsync()
    {
        // Arrange
        CancellationToken v_CancellationToken = CancellationToken.None;
        
        GenericEntityListRequest<Theme> v_Request = new();
        

        m_UseCase = new ThemeListUseCase(m_UnitOfWorkMock.Object, m_LoggerMock.Object);

        // Act
        GenericEntityListResponse<Theme> v_Result = await m_UseCase.Handle(v_Request, v_CancellationToken);

        // Assert
        m_UnitOfWorkMock.Verify(p_Uow => p_Uow.ThemeRepository.GetAllThemesAsync(It.IsAny<CancellationToken>()), Times.Never);
        v_Result.Success.Should().BeFalse();
        v_Result.Errors.Should().NotBeNull();
        m_UnitOfWorkMock.VerifyAll();
    }
}
