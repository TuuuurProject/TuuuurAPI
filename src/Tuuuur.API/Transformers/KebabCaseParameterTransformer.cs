namespace Tuuuur.API.Transformers;

/// <summary>
/// Transformer for api calls
/// </summary>
public partial class KebabCaseParameterTransformer : IOutboundParameterTransformer
{
    /// <summary>
    ///  ctor
    /// </summary>
    /// <param name="p_Value"></param>
    /// <returns></returns>
    public string TransformOutbound(object p_Value)
    {
        return p_Value == null ? null : MyRegex().Replace(p_Value.ToString() ?? string.Empty, "$1-$2").ToLower();
    }

    [System.Text.RegularExpressions.GeneratedRegex("([a-z])([A-Z])")]
    private static partial System.Text.RegularExpressions.Regex MyRegex();
}
