using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tuuuur.API.Controllers;
using Tuuuur.API.Presenters;
using Tuuuur.API.Requests;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.API.Tests.Controllers;

public class HistoryControllerTests
{
    private readonly Mock<ILogger<HistoryController>> m_LoggerMock;
    private readonly Mock<IMediator> m_MediatorMock;
    private readonly HistoryController m_Controller;

    public HistoryControllerTests()
    {
        m_LoggerMock = new Mock<ILogger<HistoryController>>();
        m_MediatorMock = new Mock<IMediator>();
        m_Controller = new HistoryController(m_LoggerMock.Object, m_MediatorMock.Object, new ValidationPresenter());
    }

    [Fact]
    public async Task GetHistoryAsync_ReturnsOkObjectResult()
    {
        // Arrange
        PaginationRequest v_Request = new()
        {
            Page = 1,
            Size = 10,
        };

        m_MediatorMock.Setup(p_P => p_P.Send(It.IsAny<GetHistoryRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GenericEntityResponse<HistoryPage>(new HistoryPage()));
            
        // Act
        IActionResult v_Result = await m_Controller.GetHistoryAsync(v_Request, new PaginationRequestValidator(), new GenericEntityPresenter<HistoryPage>(), CancellationToken.None);

        // Assert
        v_Result.Should().BeOfType<JsonContentResult>();
        ContentResult v_ContentResult = v_Result.As<ContentResult>();
        v_ContentResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }
    
    [Fact]
    public async Task GetHistoryAsync_WhenRequestIsInvalid_ReturnsOkObjectResult()
    {
        // Arrange
        PaginationRequest v_Request = new();

        m_MediatorMock.Setup(p_P => p_P.Send(It.IsAny<GetHistoryRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GenericEntityResponse<HistoryPage>(new HistoryPage()));
            
        // Act
        IActionResult v_Result = await m_Controller.GetHistoryAsync(v_Request, new PaginationRequestValidator(), new GenericEntityPresenter<HistoryPage>(), CancellationToken.None);

        // Assert
        v_Result.Should().BeOfType<JsonContentResult>();
        ContentResult v_ContentResult = v_Result.As<ContentResult>();
        v_ContentResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }
}
