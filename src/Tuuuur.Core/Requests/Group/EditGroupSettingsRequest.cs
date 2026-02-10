using MediatR;
using Tuuuur.Core.Responses;

namespace Tuuuur.Core.Requests.Group;

public record  EditGroupSettingsRequest(int[] ThemesIds, int[] DifficultiesIds, int NbQuestions, bool ScoreEachRound): IRequest<EmptyResponse>;