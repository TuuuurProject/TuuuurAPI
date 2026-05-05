using System.Diagnostics.CodeAnalysis;
using Tuuuur.Domain.Interfaces.Emails;

namespace Tuuuur.Domain.Emails.Models;

/// <summary>
/// Class of user loosing password
/// </summary>
[ExcludeFromCodeCoverage]
public class TwoFactorAuthModel : ConfirmAccountModel;
