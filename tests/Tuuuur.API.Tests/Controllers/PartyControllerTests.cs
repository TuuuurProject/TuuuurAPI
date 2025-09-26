using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Tuuuur.API.Controllers;
using Tuuuur.API.Presenters;
using Tuuuur.API.Requests;
using Tuuuur.Core.Requests;
using Tuuuur.Core.Responses;

namespace Tuuuur.API.Tests.Controllers
{
    public class PartyControllerTests
    {
        private readonly Mock<ILogger<PartyController>> m_LoggerMock;
        private readonly Mock<IMediator> m_MediatorMock;
        private readonly PartyController m_Controller;

        public PartyControllerTests()
        {
            m_LoggerMock = new Mock<ILogger<PartyController>>();
            m_MediatorMock = new Mock<IMediator>();
            m_Controller = new PartyController(m_LoggerMock.Object, m_MediatorMock.Object, new ValidationPresenter());
        }
        [Fact]
        public async Task CreateSoloPartyAsync_ReturnsOkObjectResult()
        {
            // Arrange
            CreateSoloPartyApiRequest v_ApiRequest = new()
            {
                NbQuestions = 30,
                Themes = [1,2,3],
                Difficulties = [1]
            };
            Guid v_Id = Guid.NewGuid();

            m_MediatorMock.Setup(p_P => p_P.Send(It.IsAny<CreateSoloPartyRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GuidResponse(v_Id));
            
            // Act
            IActionResult v_Result = await m_Controller.CreateSoloPartyAsync(v_ApiRequest, new CreateSoloPartyRequestValidator(), new GuidPresenter(), CancellationToken.None);

            // Assert
            v_Result.Should().BeOfType<JsonContentResult>();
            if (v_Result is JsonContentResult v_JsonResult)
            {
                v_JsonResult.StatusCode.Should().Be(200);
                v_JsonResult.Content.Should().Contain(v_Id.ToString());
            }
        }
    }
}