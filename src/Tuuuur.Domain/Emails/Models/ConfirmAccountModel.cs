using System.Diagnostics.CodeAnalysis;
using Tuuuur.Domain.Interfaces.Emails;

namespace Tuuuur.Domain.Emails.Models;

/// <summary>
/// Class of user loosing password
/// </summary>
[ExcludeFromCodeCoverage]
public class ConfirmAccountModel : BaseEmailModel, IRenderModel
{
    /// <summary>
    /// User Fullname
    /// </summary>
    public string NickName { get; set; } = string.Empty;

    /// <summary>
    /// Address of user confirm password
    /// </summary>
    public string ConfirmationCode { get; set; }
}
