using System;
using System.Collections.Generic;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Entities;

public partial class PartyThemePth
{
    public int Id { get; set; }

    public Guid IdParty { get; set; }

    public int IdTheme { get; set; }

    public virtual PartyPty IdPartyNavigation { get; set; }

    public virtual ThemeThm IdThemeNavigation { get; set; }
}
