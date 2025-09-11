using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tuuuur.API.Controllers;
using Tuuuur.API.Presenters;
using Tuuuur.API.Presenters.Authentication;
using Tuuuur.API.Requests;
using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Responses.Authentication;
using Tuuuur.Domain.Security;
using Tuuuur.Domain.Token;
using Tuuuur.Domain.Bo;
using Tuuuur.Core.Responses;

namespace Tuuuur.API.Tests.Controllers
{
    public class IdentityControllerTests
    {
        private readonly Mock<ILogger<IdentityController>> m_LoggerMock;
        private readonly Mock<IMediator> m_MediatorMock;
        private readonly IdentityController m_Controller;

        public IdentityControllerTests()
        {
            m_LoggerMock = new Mock<ILogger<IdentityController>>();
            m_MediatorMock = new Mock<IMediator>();
            m_Controller = new IdentityController(m_LoggerMock.Object, m_MediatorMock.Object, new ValidationPresenter());
        }

        [Fact]
        public void Get_WhenUserIsAuthenticated_ReturnsOkObjectResult()
        {
            // Arrange
            ClaimsPrincipal v_User = new(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "John Doe"),
                new Claim(ClaimTypes.Role, RolesType.Admin)
            }, "mock"));

            m_Controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = v_User }
            };

            // Act
            IActionResult v_Result = m_Controller.Get();

            // Assert
            v_Result.Should().BeOfType<OkObjectResult>();
            v_Result.As<OkObjectResult>().Value.Should().BeEquivalentTo(new
            {
                User = "John Doe",
                Claims = new[]
                {
                    new { Type = ClaimTypes.Name, Value = "John Doe" },
                    new { Type = ClaimTypes.Role, Value = RolesType.Admin }
                }
            });
        }

        [Fact]
        public void Get_WhenUserIsNotAuthenticated_ReturnsUnauthorizedResult()
        {
            // Arrange
            m_Controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            // Act
            IActionResult v_Result = m_Controller.Get();

            // Assert
            v_Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task LoginAsync_WithValidRequest_ReturnsOkObjectResultAsync()
        {
            // Arrange
            LoginRequest v_LoginRequest = new()
            {
                Email = "test@example.com",
                Password = "Password123"
            };

            UserToken v_AuthenticationResponse = new UserToken
            {
                User = new User(),
                Token = new JwtTokenResponse()
            };
            m_MediatorMock.Setup(p_M => p_M.Send(It.IsAny<JwtAuthenticationRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new JwtAuthenticationResponse(v_AuthenticationResponse));

            // Act
            IActionResult v_Result = await m_Controller.LoginAsync(v_LoginRequest, new LoginRequestValidator(), new JwtAuthenticationPresenter());

            // Assert
            v_Result.Should().BeOfType<JsonContentResult>();
        }

        [Fact]
        public async Task LoginAsync_WithInvalidRequest_ReturnsBadRequestObjectResultAsync()
        {
            // Arrange
            LoginRequest v_LoginRequest = new()
            {
                Email = "test@example.com",
                Password = "password123"
            };

            // Act
            IActionResult v_Result = await m_Controller.LoginAsync(v_LoginRequest, new LoginRequestValidator(), new JwtAuthenticationPresenter());

            // Assert
            v_Result.Should().BeOfType<JsonContentResult>();
            JsonContentResult v_BadRequestResult = (JsonContentResult)v_Result;
            IEnumerable<ErrorDto> v_Errors = JsonSerializer.Deserialize<IEnumerable<ErrorDto>>(v_BadRequestResult.Content);
            v_Errors.Should().NotBeEmpty();
        }
    }
}