using System;
using System.Linq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tuuuur.API.Controllers;
using Tuuuur.API.Presenters;
using Tuuuur.API.Requests;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Requests.Parties;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.API.Tests.Controllers
{
    public class SoloControllerTests
    {
        private readonly Mock<ILogger<SoloController>> m_LoggerMock;
        private readonly Mock<IMediator> m_MediatorMock;
        private readonly SoloController m_Controller;

        public SoloControllerTests()
        {
            m_LoggerMock = new Mock<ILogger<SoloController>>();
            m_MediatorMock = new Mock<IMediator>();
            m_Controller = new SoloController(m_LoggerMock.Object, m_MediatorMock.Object, new ValidationPresenter());
        }

        [Fact]
        public async Task CreateSoloPartyAsync_ReturnsOkObjectResult()
        {
            // Arrange
            SoloSettingsRequest v_ApiRequest = new()
            {
                NbQuestions = 20,
                Themes = [1, 2, 3],
                Difficulties = [1]
            };
            Guid v_Id = Guid.NewGuid();

            m_MediatorMock.Setup(p_P => p_P.Send(It.IsAny<CreateSoloPartyRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GuidResponse(v_Id));

            // Act
            IActionResult v_Result = await m_Controller.CreateSoloPartyAsync(v_ApiRequest, new SoloSettingsRequestValidator(), new GuidPresenter(), CancellationToken.None);

            // Assert
            v_Result.Should().BeOfType<JsonContentResult>();
            v_Result.Should().BeOfType<JsonContentResult>();
            ContentResult v_ContentResult = v_Result.As<ContentResult>();
            v_ContentResult.StatusCode.Should().Be(StatusCodes.Status200OK);
            v_ContentResult.Content.Should().Contain(v_Id.ToString());
        }

        [Fact]
        public async Task GetPartyStateAsync_ReturnsOkObjectResult()
        {
            // Arrange
            Guid v_Id = Guid.NewGuid();

            PartyBase v_Party = new();

            m_MediatorMock.Setup(p_P => p_P.Send(It.IsAny<GetSoloPartyStateRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GenericEntityResponse<PartyBase>(v_Party));

            // Act
            IActionResult v_Result = await m_Controller.GetPartyStateAsync(v_Id, new GenericEntityPresenter<PartyBase>(), CancellationToken.None);

            // Assert
            v_Result.Should().BeOfType<JsonContentResult>();
            ContentResult v_ContentResult = v_Result.As<ContentResult>();
            v_ContentResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task UpdatePartyStateAsync_ReturnsOkObjectResult()
        {
            // Arrange
            Guid v_Id = Guid.NewGuid();

            AnswerApiRequest v_Request = new()
            {
                AnswerId = 1,
            };
            PartyBase v_Party = new();

            m_MediatorMock.Setup(p_P => p_P.Send(It.IsAny<UpdateSoloPartyStateRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GenericEntityResponse<PartyBase>(v_Party));

            // Act
            IActionResult v_Result = await m_Controller.UpdatePartyStateAsync(v_Id, v_Request, new GenericEntityPresenter<PartyBase>(), CancellationToken.None);

            // Assert
            v_Result.Should().BeOfType<JsonContentResult>();
            ContentResult v_ContentResult = v_Result.As<ContentResult>();
            v_ContentResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        }
    }
}