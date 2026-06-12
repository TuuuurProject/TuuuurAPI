using System.Collections.Generic;
using Tuuuur.Domain.Bo;

namespace Tuuuur.Domain.Tests.Bo;

public class RankingPageTests
{
    [Fact]
    public void RankingPage_DefaultValues_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        RankingPage v_Page = new();

        // Assert
        v_Page.Users.Should().BeEmpty();
        v_Page.UserRanking.Should().Be(0);
        v_Page.UserElo.Should().Be(0);
        v_Page.CurrentPage.Should().Be(0);
        v_Page.TotalPages.Should().Be(0);
        v_Page.TotalUsers.Should().Be(0);
        v_Page.UserTier.Should().Be(0);
        v_Page.UserDivision.Should().Be(0);
    }

    [Fact]
    public void RankingPage_ShouldStoreProperties()
    {
        // Arrange
        List<User> v_Users = [new User { Id = Guid.NewGuid() }];

        // Act
        RankingPage v_Page = new()
        {
            Users = v_Users,
            UserRanking = 5,
            UserElo = 1200,
            CurrentPage = 2,
            TotalPages = 10,
            TotalUsers = 100,
            UserTier = 3,
            UserDivision = 1
        };

        // Assert
        v_Page.Users.Should().HaveCount(1);
        v_Page.UserRanking.Should().Be(5);
        v_Page.UserElo.Should().Be(1200);
        v_Page.CurrentPage.Should().Be(2);
        v_Page.TotalPages.Should().Be(10);
        v_Page.TotalUsers.Should().Be(100);
        v_Page.UserTier.Should().Be(3);
        v_Page.UserDivision.Should().Be(1);
    }

    [Fact]
    public void RankingPage_WithExpression_ShouldCreateModifiedCopy()
    {
        // Arrange
        RankingPage v_Original = new()
        {
            UserTier = 1,
            UserDivision = 3,
            UserRanking = 10
        };

        // Act — uses the 'with' expression (same as GetRankingUseCase does)
        RankingPage v_Modified = v_Original with
        {
            UserTier = 5,
            UserDivision = 1
        };

        // Assert — modified fields updated, others preserved
        v_Modified.UserTier.Should().Be(5);
        v_Modified.UserDivision.Should().Be(1);
        v_Modified.UserRanking.Should().Be(10);
    }
}
