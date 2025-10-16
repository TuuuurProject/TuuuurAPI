using System;
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
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

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

            Party v_Party = new();

            m_MediatorMock.Setup(p_P => p_P.Send(It.IsAny<GetPartyStateRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GenericEntityResponse<Party>(v_Party));
            
            // Act
            IActionResult v_Result = await m_Controller.GetPartyStateAsync(v_Id, new GenericEntityPresenter<Party>(), CancellationToken.None);

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
            
            AnwserApiRequest v_Request = new()
            {
                AnwserId = 1,
            };
            Party v_Party = new();

            m_MediatorMock.Setup(p_P => p_P.Send(It.IsAny<UpdatePartyStateRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GenericEntityResponse<Party>(v_Party));
            
            // Act
            IActionResult v_Result = await m_Controller.UpdatePartyStateAsync(v_Id, v_Request, new AnwserApiRequestValidator(), new GenericEntityPresenter<Party>(), CancellationToken.None);

            // Assert
            v_Result.Should().BeOfType<JsonContentResult>();
            ContentResult v_ContentResult = v_Result.As<ContentResult>();
            v_ContentResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        }
    }
}