namespace Tuuuur.Domain.Bo;

/// <summary>
/// Describes the question pool to use for a ranked match:
/// which difficulty IDs are allowed and whether questions are restricted to specific themes.
/// </summary>
/// <param name="DifficultyIds">
/// Allowed difficulty IDs (e.g. 1=Facile, 2=Moyen, 3=Difficile, 4=Extrême).
/// When empty, no difficulty filter is applied.
/// </param>
/// <param name="ThemeIds">
/// Optional theme filter. When <c>null</c> or empty, all themes are eligible.
/// When set, only questions associated with one of these theme IDs are returned.
/// </param>
public record RankedQuestionPool(int[] DifficultyIds, int[]? ThemeIds);
