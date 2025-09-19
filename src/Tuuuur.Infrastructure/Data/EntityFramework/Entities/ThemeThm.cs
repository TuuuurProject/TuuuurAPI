using System;
using System.Collections.Generic;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Entities;

public partial class ThemeThm
{
    public int Id { get; set; }

    public string Icon { get; set; }

    public string Label { get; set; }

    public virtual ICollection<EloElo> EloElo { get; set; } = new List<EloElo>();

    public virtual ICollection<QuestionQst> IdQuestion { get; set; } = new List<QuestionQst>();
}
