using Tuuuur.Domain.Emails.Models;
using Tuuuur.Infrastructure.Emails;

namespace Tuuuur.Infrastructure.Tests.Email;

public class RenderingServiceTests
{
    private readonly RenderingService m_RenderingService = new();

	
    [Fact]
    public void RenderPage_ReturnsStringContent()
    {
        // Arrange
        string v_NickName = "mysupernickname";
        string v_ConfirmationCode = "123456";

        ConfirmAccountModel v_Model = new()
        {
            NickName = v_NickName,
            ConfirmationCode =  v_ConfirmationCode
        };

        // Act
        Check.ThatCode(() => m_RenderingService.RenderAsync(
            v_Model)).WhichResult().IsNotNull().And.Contains(v_NickName, v_ConfirmationCode);
    }
}
