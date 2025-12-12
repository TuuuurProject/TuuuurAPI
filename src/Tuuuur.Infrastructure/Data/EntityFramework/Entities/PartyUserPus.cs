using System;
using System.Collections.Generic;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Entities;

public partial class PartyUserPus
{
    public int Id { get; set; }

    public int IdUser { get; set; }

    public Guid IdParty { get; set; }

    public virtual PartyPty IdPartyNavigation { get; set; }

    public virtual UserUsr IdUserNavigation { get; set; }
}
