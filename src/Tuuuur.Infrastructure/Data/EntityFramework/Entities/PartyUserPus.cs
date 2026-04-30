using System;
using System.Collections.Generic;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Entities;

public partial class PartyUserPus
{
    public int Id { get; set; }

    public Guid IdUser { get; set; }

    public Guid IdParty { get; set; }

    public int? Elo { get; set; }

    public bool? Winner { get; set; }

    public int? FinalScore { get; set; }

    public virtual PartyPty IdPartyNavigation { get; set; }

    public virtual UserUsr IdUserNavigation { get; set; }
}
