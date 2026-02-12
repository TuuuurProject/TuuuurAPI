using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tuuuur.API.Controllers;
using Tuuuur.API.Presenters;
using Tuuuur.API.Requests;
using Tuuuur.Core.Requests.Group;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Factory.Tests;

namespace Tuuuur.API.Tests.Controllers;

public class GroupControllerTests
{
    private readonly Mock<ILogger<GroupController>> m_LoggerMock;
    private readonly Mock<IMediator> m_MediatorMock;
    private readonly GroupController m_Controller;

    public GroupControllerTests()
    {
        m_LoggerMock = new Mock<ILogger<GroupController>>();
        m_MediatorMock = new Mock<IMediator>();
        m_Controller = new GroupController(m_LoggerMock.Object, m_MediatorMock.Object, new ValidationPresenter());
    }
    
    [Fact]
    public async Task CreatePartyAsync_ReturnsOkObjectResult()
    {
        // Arrange
        GroupParty v_Party = BoFactory.CreateGroupParty();
        m_MediatorMock.Setup(p_P => p_P.Send(It.IsAny<CreateGroupPartyRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GenericEntityResponse<GroupParty>(v_Party));
        
        // Act
        IActionResult v_Result = await m_Controller.CreatePartyAsync(new GenericEntityPresenter<GroupParty>(), CancellationToken.None);

        // Assert
        v_Result.Should().BeOfType<JsonContentResult>();
        ContentResult v_ContentResult = v_Result.As<ContentResult>();
        v_ContentResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }
    
    [Fact]
    public async Task JoinPartyAsync_ReturnsOkObjectResult()
    {
        // Arrange
        CodeRequest v_Request = new(){ Code = "438212" };
        GroupParty v_Party = BoFactory.CreateGroupParty();
        m_MediatorMock.Setup(p_P => p_P.Send(It.IsAny<JoinGroupPartyRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GenericEntityResponse<GroupParty>(v_Party));
        
        // Act
        IActionResult v_Result = await m_Controller.JoinPartyAsync(v_Request, new GenericEntityPresenter<GroupParty>(), CancellationToken.None);

        // Assert
        v_Result.Should().BeOfType<JsonContentResult>();
        ContentResult v_ContentResult = v_Result.As<ContentResult>();
        v_ContentResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }
    
    [Fact]
    public async Task LeavePartyAsync_ReturnsOkObjectResult()
    {
        // Arrange
        m_MediatorMock.Setup(p_P => p_P.Send(It.IsAny<LeaveGroupPartyRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EmptyResponse());
        
        // Act
        IActionResult v_Result = await m_Controller.LeavePartyAsync(new EmptyPresenter(), CancellationToken.None);

        // Assert
        v_Result.Should().BeOfType<JsonContentResult>();
        ContentResult v_ContentResult = v_Result.As<ContentResult>();
        v_ContentResult.StatusCode.Should().Be(StatusCodes.Status200OK);
    }
}
