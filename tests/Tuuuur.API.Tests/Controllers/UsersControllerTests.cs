using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Tuuuur.API.Controllers;
using Tuuuur.API.Presenters;
using Tuuuur.API.Requests;
using Tuuuur.Core.Requests.Users;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Factory.Tests;

namespace Tuuuur.API.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IMediator> m_MediatorMock;
    private readonly UsersController m_Controller;

    public UsersControllerTests()
    {
        Mock<ILogger<UsersController>> v_LoggerMock = new();
        m_MediatorMock = new Mock<IMediator>();
        m_Controller = new UsersController(v_LoggerMock.Object, m_MediatorMock.Object, new ValidationPresenter());
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
}
