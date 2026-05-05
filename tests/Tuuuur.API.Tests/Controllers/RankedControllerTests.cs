using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tuuuur.API.Controllers;
using Tuuuur.API.Presenters;
using Tuuuur.Core.Requests.Ranked;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Bo.Enum;

namespace Tuuuur.API.Tests.Controllers;

public class RankedControllerTests
{
    private readonly Mock<ILogger<RankedController>> m_LoggerMock;
    private readonly Mock<IMediator> m_MediatorMock;
    private readonly RankedController m_Controller;

    public RankedControllerTests()
    {
        m_LoggerMock = new Mock<ILogger<RankedController>>();
        m_MediatorMock = new Mock<IMediator>();
        m_Controller = new RankedController(m_LoggerMock.Object, m_MediatorMock.Object, new ValidationPresenter());
    }

    [Fact]
    public async Task GetRankedPartyAsync_ReturnsOkObjectResult()
    {
        // Arrange
        Guid v_PartyId = Guid.NewGuid();
        RankedParty v_Party = new()
        {
            Id = v_PartyId,
            IdPartyType = (int)PartyTypeType.Ranked
        };

        m_MediatorMock
            .Setup(p_M => p_M.Send(It.Is<GetRankedRequest>(p_Request => p_Request.PartyId == v_PartyId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GenericEntityResponse<RankedParty>(v_Party));

        // Act
        IActionResult v_Result = await m_Controller.GetRankedPartyAsync(v_PartyId, new GenericEntityPresenter<RankedParty>(), CancellationToken.None);

        // Assert
        v_Result.Should().BeOfType<JsonContentResult>();
        ContentResult v_ContentResult = v_Result.As<ContentResult>();
        v_ContentResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        m_MediatorMock.Verify(p_M => p_M.Send(It.Is<GetRankedRequest>(p_Request => p_Request.PartyId == v_PartyId), It.IsAny<CancellationToken>()), Times.Once);
    }
}