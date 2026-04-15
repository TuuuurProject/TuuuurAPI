using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Tuuuur.Domain.Images;

[ExcludeFromCodeCoverage]
public static class Logo
{
    private const string EmbeddedResourceName = "Tuuuur.Domain.Images.Logo.png";

    /// <summary>
    /// Retourne le contenu de l'image en base64 brut à partir de la ressource embarquée.
    /// </summary>
    private static string GetBase64()
    {
        try
        {
            Assembly v_Assembly = typeof(Logo).Assembly;
            using Stream v_Stream = v_Assembly.GetManifestResourceStream(EmbeddedResourceName);
            if (v_Stream == null)
                return string.Empty;

            using MemoryStream v_MemoryStream = new();
            v_Stream.CopyTo(v_MemoryStream);
            byte[] v_Bytes = v_MemoryStream.ToArray();
            return Convert.ToBase64String(v_Bytes);
        }
        catch
        {
            return string.Empty;
        }
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