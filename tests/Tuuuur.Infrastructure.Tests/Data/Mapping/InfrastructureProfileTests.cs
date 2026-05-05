using System;
using AutoMapper;
using Tuuuur.Domain.Bo;
using Tuuuur.Domain.Bo.Enum;
using Tuuuur.Infrastructure.Data.EntityFramework.Entities;
using Tuuuur.Infrastructure.Data.Mapping;

namespace Tuuuur.Infrastructure.Tests.Data.Mapping
{
    public class InfrastructureProfileTests
    {
        public static MapperConfiguration InitializeAutoMapper()
        {
            return new MapperConfiguration(p_Cfg =>
                p_Cfg.AddProfile(new InfrastructureProfile())
            );  //mapping between EF Core Entity and Business layer objects
        }

        private static PartyPty CreateRankedPartyEntity(Guid p_CurrentUserId, Guid p_OtherUserId)
        {
            Guid v_PartyId = Guid.NewGuid();

            return new PartyPty
            {
                Id = v_PartyId,
                Dt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IdPartyType = (int)PartyTypeType.Ranked,
                IdUserHost = p_CurrentUserId,
                Active = true,
                Finish = false,
                PartyUserPus =
                [
                    new PartyUserPus
                    {
                        Id = 1,
                        IdParty = v_PartyId,
                        IdUser = p_CurrentUserId,
                        Elo = 1240,
                        Winner = true,
                        FinalScore = 42,
                        IdUserNavigation = new UserUsr { Id = p_CurrentUserId, NickName = "Current" }
                    },
                    new PartyUserPus
                    {
                        Id = 2,
                        IdParty = v_PartyId,
                        IdUser = p_OtherUserId,
                        Elo = 980,
                        Winner = false,
                        FinalScore = 12,
                        IdUserNavigation = new UserUsr { Id = p_OtherUserId, NickName = "Other" }
                    }
                ],
                PartyQuestionPqt =
                [
                    new PartyQuestionPqt
                    {
                        Id = 10,
                        IdParty = v_PartyId,
                        IdQuestion = 100,
                        Order = 1,
                        UserPartyQuestionUpq =
                        [
                            new UserPartyQuestionUpq
                            {
                                Id = 1000,
                                IdPartyQuestion = 10,
                                IdUser = p_OtherUserId,
                                DtPresentedAt = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                                DtAnsweredAt = new DateTime(2024, 1, 1, 10, 0, 10, DateTimeKind.Utc),
                                Score = 5,
                                AnswersOrder = Guid.NewGuid(),
                                IdUserNavigation = new UserUsr { Id = p_OtherUserId, NickName = "Other" }
                            },
                            new UserPartyQuestionUpq
                            {
                                Id = 1001,
                                IdPartyQuestion = 10,
                                IdUser = p_CurrentUserId,
                                DtPresentedAt = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                                DtAnsweredAt = new DateTime(2024, 1, 1, 10, 0, 8, DateTimeKind.Utc),
                                Score = 30,
                                AnswersOrder = Guid.NewGuid(),
                                IdUserNavigation = new UserUsr { Id = p_CurrentUserId, NickName = "Current" }
                            }
                        ]
                    },
                    new PartyQuestionPqt
                    {
                        Id = 11,
                        IdParty = v_PartyId,
                        IdQuestion = 101,
                        Order = 2,
                        UserPartyQuestionUpq =
                        [
                            new UserPartyQuestionUpq
                            {
                                Id = 1002,
                                IdPartyQuestion = 11,
                                IdUser = p_OtherUserId,
                                DtPresentedAt = new DateTime(2024, 1, 1, 10, 1, 0, DateTimeKind.Utc),
                                DtAnsweredAt = new DateTime(2024, 1, 1, 10, 1, 12, DateTimeKind.Utc),
                                Score = 7,
                                AnswersOrder = Guid.NewGuid(),
                                IdUserNavigation = new UserUsr { Id = p_OtherUserId, NickName = "Other" }
                            },
                            new UserPartyQuestionUpq
                            {
                                Id = 1003,
                                IdPartyQuestion = 11,
                                IdUser = p_CurrentUserId,
                                DtPresentedAt = new DateTime(2024, 1, 1, 10, 1, 0, DateTimeKind.Utc),
                                DtAnsweredAt = new DateTime(2024, 1, 1, 10, 1, 9, DateTimeKind.Utc),
                                Score = 20,
                                AnswersOrder = Guid.NewGuid(),
                                IdUserNavigation = new UserUsr { Id = p_CurrentUserId, NickName = "Current" }
                            }
                        ]
                    }
                ]
            };
        }

        [Fact]
        public void ValidateProfile()
        {
            Check.ThatCode(() =>
                new MapperConfiguration(p_Cfg =>
                        p_Cfg.AddProfile(typeof(InfrastructureProfile)))
                    .AssertConfigurationIsValid()
            ).DoesNotThrow();
        }

        [Fact]
        public void Map_PartyToRankedParty_WhenCurrentUserIdIsProvided_ShouldPopulateRankedFields()
        {
            // Arrange
            IMapper v_Mapper = InitializeAutoMapper().CreateMapper();
            Guid v_CurrentUserId = Guid.NewGuid();
            Guid v_OtherUserId = Guid.NewGuid();
            PartyPty v_Source = CreateRankedPartyEntity(v_CurrentUserId, v_OtherUserId);

            // Act
            RankedParty v_Result = v_Mapper.Map<RankedParty>(v_Source, p_Opt =>
                p_Opt.Items[$"{nameof(User)}.{nameof(User.Id)}"] = v_CurrentUserId);

            // Assert
            v_Result.NbQuestions.Should().Be(2);
            v_Result.UserScores.Should().HaveCount(2);
            v_Result.UserScores[0].Score.Should().Be(50);
            v_Result.UserScores[0].User.Id.Should().Be(v_CurrentUserId);
            v_Result.UserScores[1].Score.Should().Be(12);
            v_Result.UserScores[1].User.Id.Should().Be(v_OtherUserId);
            v_Result.PartyQuestions.Should().HaveCount(2);
            v_Result.IsWinner.Should().BeTrue();
            v_Result.Elo.Should().Be(1240);
            v_Result.FinalScore.Should().Be(42);
        }

        [Fact]
        public void Map_PartyToRankedParty_WhenCurrentUserIdIsMissing_ShouldKeepDefaultRankedFields()
        {
            // Arrange
            IMapper v_Mapper = InitializeAutoMapper().CreateMapper();
            Guid v_CurrentUserId = Guid.NewGuid();
            Guid v_OtherUserId = Guid.NewGuid();
            PartyPty v_Source = CreateRankedPartyEntity(v_CurrentUserId, v_OtherUserId);

            // Act
            RankedParty v_Result = v_Mapper.Map<RankedParty>(v_Source);

            // Assert
            v_Result.NbQuestions.Should().Be(2);
            v_Result.UserScores.Should().HaveCount(2);
            v_Result.UserScores[0].Score.Should().Be(50);
            v_Result.UserScores[0].User.Id.Should().Be(v_CurrentUserId);
            v_Result.UserScores[1].Score.Should().Be(12);
            v_Result.UserScores[1].User.Id.Should().Be(v_OtherUserId);
            v_Result.PartyQuestions.Should().HaveCount(2);
            v_Result.IsWinner.Should().BeFalse();
            v_Result.Elo.Should().Be(0);
            v_Result.FinalScore.Should().Be(0);
        }
    }
}