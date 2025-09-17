using System.Diagnostics.CodeAnalysis;

namespace Tuuuur.Domain.Images;

[ExcludeFromCodeCoverage]
public static class Logo
{
    private static readonly string m_RelativePath = Path.Combine("src", "Tuuuur.Domain", "Images", "Logo.png");

    /// <summary>
    /// Retourne le chemin absolu vers le logo.
    /// </summary>
    public static string GetFullPath()
    {
        string v_SolutionRoot = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..");
        return Path.GetFullPath(Path.Combine(v_SolutionRoot, m_RelativePath));
    }

    /// <summary>
    /// Retourne le contenu de l'image en base64 brut.
    /// </summary>
    private static string GetBase64()
    {
        string v_FullPath = GetFullPath();

        if (!File.Exists(v_FullPath))
            return string.Empty;

        byte[] v_Bytes = File.ReadAllBytes(v_FullPath);
        return Convert.ToBase64String(v_Bytes);
    }

    /// <summary>
    /// Retourne un "data URI" (data:image/png;base64,...)
    /// </summary>
    public static string GetDataUri()
    {
        string v_Base64 = GetBase64();
        return string.IsNullOrEmpty(v_Base64) ? string.Empty : $"data:image/png;base64,{v_Base64}";
    }
}