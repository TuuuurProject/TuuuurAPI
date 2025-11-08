using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Tuuuur.API.Controllers;
using Tuuuur.API.Presenters;
using Tuuuur.API.Requests;
using Tuuuur.API.Requests.Users;
using Tuuuur.Core.Requests.Users;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Factory.Tests;

namespace Tuuuur.API.Tests.Controllers;

public class MeControllerTests
{
    private readonly Mock<IMediator> m_MediatorMock;
    private readonly MeController m_Controller;

    public MeControllerTests()
    {
        Mock<ILogger<MeController>> v_LoggerMock = new();
        m_MediatorMock = new Mock<IMediator>();
        m_Controller = new MeController(v_LoggerMock.Object, m_MediatorMock.Object, new ValidationPresenter());
    }
    
    [Fact]
    public async Task UpdateUserAvatarAsync_ReturnsOkObjectResult()
    {
        // Arrange
        UserAvatarApiRequest v_ApiRequest = new()
        {
            Avatar = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR4nGNgYAAAAAMAASsJTYQAAAAASUVORK5CYII="
        };
        User v_User = BoFactory.CreateUser();

        m_MediatorMock.Setup(p_P => p_P.Send(It.IsAny<UpdateUserAvatarRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GenericEntityResponse<User>(v_User));
        
        // Act
        IActionResult v_Result = await m_Controller.UpdateUserAvatarAsync(v_ApiRequest, new UserAvatarApiRequestValidator(), new GenericEntityPresenter<User>(), CancellationToken.None);

        // Assert
        v_Result.Should().BeOfType<JsonContentResult>();
        if (v_Result is JsonContentResult v_JsonResult)
        {
            v_JsonResult.StatusCode.Should().Be(200);
        }
    }
    
    [Fact]
    public async Task UpdateUserAvatarAsync_WhenAvatarIsNotInGoodFormat_ReturnsOkObjectResult()
    {
        // Arrange
        UserAvatarApiRequest v_ApiRequest = new()
        {
            Avatar = "BadImageFormat"
        };
        User v_User = BoFactory.CreateUser();

        m_MediatorMock.Setup(p_P => p_P.Send(It.IsAny<UpdateUserAvatarRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GenericEntityResponse<User>(v_User));
        
        // Act
        IActionResult v_Result = await m_Controller.UpdateUserAvatarAsync(v_ApiRequest, new UserAvatarApiRequestValidator(), new GenericEntityPresenter<User>(), CancellationToken.None);

        // Assert
        v_Result.Should().BeOfType<JsonContentResult>();
        if (v_Result is JsonContentResult v_JsonResult)
        {
            v_JsonResult.StatusCode.Should().Be(400);
        }
    }
    
    [Fact]
    public async Task GetUserAsync_ReturnsOkObjectResult()
    {
        // Arrange
        User v_User = BoFactory.CreateUser();

        m_MediatorMock.Setup(p_P => p_P.Send(It.IsAny<GetCurrentUserRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GenericEntityResponse<User>(v_User));
        
        // Act
        IActionResult v_Result = await m_Controller.GetUserAsync(new GenericEntityPresenter<User>(), CancellationToken.None);

        // Assert
        v_Result.Should().BeOfType<JsonContentResult>();
        if (v_Result is JsonContentResult v_JsonResult)
        {
            v_JsonResult.StatusCode.Should().Be(200);
        }
    }
    
    [Fact]
    public async Task UpdateUserPasswordAsync_ReturnsOkObjectResult()
    {
        // Arrange
        ChangePasswordApiRequest v_ApiRequest = new()
        {
            CurrentPassword = "MySuperP0swuadi!",
            NewPassword = "MyNewSuperPassword907)",
        };
        User v_User = BoFactory.CreateUser();

        m_MediatorMock.Setup(p_P => p_P.Send(It.IsAny<ChangePasswordRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GenericEntityResponse<User>(v_User));
        
        // Act
        IActionResult v_Result = await m_Controller.UpdateUserPasswordAsync(v_ApiRequest, new ChangePasswordRequestValidator(), new GenericEntityPresenter<User>(), CancellationToken.None);

        // Assert
        v_Result.Should().BeOfType<JsonContentResult>();
        if (v_Result is JsonContentResult v_JsonResult)
        {
            v_JsonResult.StatusCode.Should().Be(200);
        }
    }
    
    [Fact]
    public async Task DeleteUserAsync_ReturnsOkObjectResult()
    {
        // Arrange
        ChangePasswordApiRequest v_ApiRequest = new()
        {
            CurrentPassword = "currentpassword",
            NewPassword = "newpassword",
        };
        User v_User = BoFactory.CreateUser();

        m_MediatorMock.Setup(p_P => p_P.Send(It.IsAny<DeleteUserRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EmptyResponse());
        
        // Act
        IActionResult v_Result = await m_Controller.DeleteUserAsync(new EmptyPresenter(), CancellationToken.None);

        // Assert
        v_Result.Should().BeOfType<JsonContentResult>();
        if (v_Result is JsonContentResult v_JsonResult)
        {
            v_JsonResult.StatusCode.Should().Be(200);
        }
    }
}
