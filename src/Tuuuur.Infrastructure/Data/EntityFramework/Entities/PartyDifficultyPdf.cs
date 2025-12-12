using System;
using System.Collections.Generic;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Entities;

public partial class PartyDifficultyPdf
{
    public int Id { get; set; }

    public Guid IdParty { get; set; }

    public int IdDifficulty { get; set; }

    public virtual DifficultyDft IdDifficultyNavigation { get; set; }

    public virtual PartyPty IdPartyNavigation { get; set; }
}
