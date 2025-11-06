namespace Tuuuur.API.Transformers;

/// <summary>
/// Transformer for api calls
/// </summary>
public partial class KebabCaseParameterTransformer : IOutboundParameterTransformer
{
    /// <summary>
    ///  ctor
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [SuppressMessage("Style", "IDE1006:Styles d'affectation de noms", Justification = "Inherited named")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public string TransformOutbound(object value)
    {
        return value == null ? null : MyRegex().Replace(value.ToString() ?? string.Empty, "$1-$2").ToLower();
    }

    [System.Text.RegularExpressions.GeneratedRegex("([a-z])([A-Z])")]
    private static partial System.Text.RegularExpressions.Regex MyRegex();
}
