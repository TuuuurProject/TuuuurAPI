using System;
using System.Collections.Generic;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Entities;

public partial class DifficultyDft
{
    public int Id { get; set; }

    public string Label { get; set; }

    public virtual ICollection<PartyDifficultyPdf> PartyDifficultyPdf { get; set; } = new List<PartyDifficultyPdf>();

    public virtual ICollection<QuestionQst> QuestionQst { get; set; } = new List<QuestionQst>();
}
