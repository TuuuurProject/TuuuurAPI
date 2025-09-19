using System;
using System.Collections.Generic;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Entities;

public partial class EloElo
{
    public int IdUser { get; set; }

    public int IdTheme { get; set; }

    public int Value { get; set; }

    public virtual ThemeThm IdThemeNavigation { get; set; }

    public virtual UserUsr IdUserNavigation { get; set; }
}
