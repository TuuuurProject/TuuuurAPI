using MediatR;
using Tuuuur.Core.Responses;

namespace Tuuuur.Core.Requests;

public record CreateSoloPartyRequest(int[] ThemesIds, int[] DifficultiesIds, int NbQuestions): IRequest<GuidResponse>;