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
using Tuuuur.Core.Requests;
using Tuuuur.Core.Requests.Authentication;
using Tuuuur.Core.Responses.Authentication;
using Tuuuur.Domain.Security;
using Tuuuur.Domain.Token;
using Tuuuur.Domain.Bo;
using Tuuuur.Core.Responses;
using LoginRequest = Tuuuur.Core.Requests.Authentication.LoginRequest;

namespace Tuuuur.API.Tests.Controllers
{
    public class ThemeControllerTests
    {
        private readonly Mock<ILogger<ThemeController>> m_LoggerMock;
        private readonly Mock<IMediator> m_MediatorMock;
        private readonly ThemeController m_Controller;

        public ThemeControllerTests()
        {
            m_LoggerMock = new Mock<ILogger<ThemeController>>();
            m_MediatorMock = new Mock<IMediator>();
            m_Controller = new ThemeController(m_LoggerMock.Object, m_MediatorMock.Object, new ValidationPresenter());
        }
        [Fact]
        public async Task GetAllThemes_ReturnsOkObjectResultAsync()
        {
            // Arrange
            m_MediatorMock.Setup(p_M =>
                p_M.Send(It.IsAny<GenericEntityListRequest<Theme>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GenericEntityListResponse<Theme>(new List<Theme>()));
            
            // Act
            IActionResult v_Result = await m_Controller.GetThemeListAsync(new GenericEntityListPresenter<Theme>(), CancellationToken.None);

            // Assert
            v_Result.Should().BeOfType<JsonContentResult>();
            ContentResult v_ContentResult = v_Result.As<ContentResult>();
            v_ContentResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        }
    }
}