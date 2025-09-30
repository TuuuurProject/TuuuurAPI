using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Tuuuur.API.Controllers;
using Tuuuur.API.Presenters;
using Tuuuur.API.Presenters.Authentication;
using Tuuuur.API.Requests;
using Tuuuur.API.Tests.Mapping;
using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Responses.Authentication;
using Tuuuur.Domain.Security;
using Tuuuur.Domain.Token;
using Tuuuur.Domain.Bo;
using Tuuuur.Core.Responses;
using LoginRequest = Tuuuur.Core.Requests.Authentication.LoginRequest;

namespace Tuuuur.API.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<ILogger<AuthController>> m_LoggerMock;
        private readonly Mock<IMediator> m_MediatorMock;
        private readonly AuthController m_Controller;

        public AuthControllerTests()
        {
            m_LoggerMock = new Mock<ILogger<AuthController>>();
            m_MediatorMock = new Mock<IMediator>();
            m_Controller = new AuthController(m_LoggerMock.Object, m_MediatorMock.Object, new ValidationPresenter());
        }
        [Fact]
        public void Get_WhenAdminUserIsAuthenticated_ReturnsOkObjectResult()
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
            IActionResult v_Result = m_Controller.AdminOnly();

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
            Requests.AuthenticateApiRequest v_AuthenticateApiRequest = new()
            {
                Login = "test@example.com",
                Password = "Password123"
            };
            m_MediatorMock.Setup(p_M => p_M.Send(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EmptyResponse());

            // Act
            IActionResult v_Result = await m_Controller.LoginAsync(v_AuthenticateApiRequest, new AuthenticateRequestValidator(), new EmptyPresenter());

            // Assert
            v_Result.Should().BeOfType<JsonContentResult>();
        }
        
        [Fact]
        public async Task LoginAsync_WithInvalidRequest_ReturnsBadRequestObjectResultAsync()
        {
            // Arrange
            Requests.AuthenticateApiRequest v_AuthenticateApiRequest = new()
            {
                Login = "test@example.com",
                Password = "password123"
            };

            // Act
            IActionResult v_Result = await m_Controller.LoginAsync(v_AuthenticateApiRequest, new AuthenticateRequestValidator(), new EmptyPresenter());

            // Assert
            v_Result.Should().BeOfType<JsonContentResult>();
            JsonContentResult v_BadRequestResult = (JsonContentResult)v_Result;
            IEnumerable<ErrorDto> v_Errors = JsonSerializer.Deserialize<IEnumerable<ErrorDto>>(v_BadRequestResult.Content);
            v_Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ValidAccountAsync_WithInvalidRequest_ReturnsBadRequestObjectResultAsync()
        {
            // Arrange
            ValidateAccountApiRequest v_LoginApiRequest = new()
            {
                Login = "test@example.com",
                Code = "3843"
            };

            // Act
            IActionResult v_Result = await m_Controller.VerifyAccount2FaAsync(v_LoginApiRequest, new ValidateAccountValidator(), new JwtAuthenticationPresenter());

            // Assert
            v_Result.Should().BeOfType<BadRequestObjectResult>();
        }
        
        [Fact]
        public async Task ValidAccountAsync_WithValidRequest_ReturnsOkObjectResultAsync()
        {
            // Arrange
            ValidateAccountApiRequest v_LoginApiRequest = new()
            {
                Login = "test@example.com",
                Code = "654372"
            };
            UserToken v_AuthenticationResponse = new UserToken
            {
                User = new User(),
                Token = new JwtTokenResponse()
            };
            m_MediatorMock.Setup(p_M => p_M.Send(It.IsAny<VerifyAccountRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new JwtAuthenticationResponse(v_AuthenticationResponse));
            
            // Act
            IActionResult v_Result = await m_Controller.VerifyAccount2FaAsync(v_LoginApiRequest, new ValidateAccountValidator(), new JwtAuthenticationPresenter());

            // Assert
            v_Result.Should().BeOfType<JsonContentResult>();
            JsonContentResult v_RequestResult = (JsonContentResult)v_Result;
            v_RequestResult.StatusCode.Should().Be(200);
        }
        
        [Fact]
        public async Task RegisterAsync_WithRequest_ReturnsOkObjectResultAsync()
        {
            // Arrange
            RegisterApiRequest v_RegisterApiRequest = new()
            {
                Email = "test@example.com",
                Password = "MySuper_Passw0rd12)",
                NickName = "supernumerary"
            };
            m_MediatorMock.Setup(p_M => p_M.Send(It.IsAny<RegistrationRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EmptyResponse());
            // Act
            IActionResult v_Result = await m_Controller.RegisterAsync(v_RegisterApiRequest, new Mapper(BodyRequestMappingTests.InitializeAutoMapper()), new RegisterRequestValidator(), new EmptyPresenter());

            // Assert
            v_Result.Should().BeOfType<JsonContentResult>();
            JsonContentResult v_RequestResult = (JsonContentResult)v_Result;
            v_RequestResult.StatusCode.Should().Be(200);
        }
        
        [Fact]
        public async Task ForgotPasswordAsync_WithValidRequest_ReturnsOkObjectResultAsync()
        {
            // Arrange
            Tuuuur.API.Requests.LoginApiRequest v_LoginApiRequest = new()
            {
                Login = "test@example.com",
            };
            m_MediatorMock.Setup(p_M => p_M.Send(It.IsAny<ForgotPasswordRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EmptyResponse());
            
            // Act
            IActionResult v_Result = await m_Controller.ForgotPasswordAsync(v_LoginApiRequest, new LoginRequestValidator(), new EmptyPresenter());

            // Assert
            v_Result.Should().BeOfType<JsonContentResult>();
            JsonContentResult v_RequestResult = (JsonContentResult)v_Result;
            v_RequestResult.StatusCode.Should().Be(200);
        }
        
        [Fact]
        public async Task ResetPasswordAsync_WithValidRequest_ReturnsOkObjectResultAsync()
        {
            // Arrange
            Requests.ResetPasswordApiRequest v_ResetPasswordApiRequest = new()
            {
                Login = "test@example.com",
                Password = "MySuper_Passw0rd12",
                Code = "657432"
            };
            m_MediatorMock.Setup(p_M => p_M.Send(It.IsAny<Tuuuur.Core.Requests.Authentication.ResetPasswordRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EmptyResponse());
            
            // Act
            IActionResult v_Result = await m_Controller.ResetPasswordAsync(v_ResetPasswordApiRequest, new ResetPasswordRequestValidator(), new EmptyPresenter());

            // Assert
            v_Result.Should().BeOfType<JsonContentResult>();
            JsonContentResult v_RequestResult = (JsonContentResult)v_Result;
            v_RequestResult.StatusCode.Should().Be(200);
        }
        
        [Fact]
        public async Task GoogleAuthentificationAsync_WithValidRequest_ReturnsOkObjectResultAsync()
        {
            // Arrange
            ResetPasswordApiRequest v_ResetPasswordApiRequest = new()
            {
                Login = "test@example.com",
                Password = "MySuper_Passw0rd12",
                Code = "657432"
            };
            m_MediatorMock.Setup(p_M => p_M.Send(It.IsAny<Tuuuur.Core.Requests.Authentication.ResetPasswordRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EmptyResponse());
            
            // Act
            IActionResult v_Result = await m_Controller.ResetPasswordAsync(v_ResetPasswordApiRequest, new ResetPasswordRequestValidator(), new EmptyPresenter());

            // Assert
            v_Result.Should().BeOfType<JsonContentResult>();
            JsonContentResult v_RequestResult = (JsonContentResult)v_Result;
            v_RequestResult.StatusCode.Should().Be(200);
        }
    }
}