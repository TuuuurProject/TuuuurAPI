using System;
using System.Collections.Generic;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Entities;

public partial class UserPartyQuestionUpq
{
    public int Id { get; set; }

    public int IdPartyQuestion { get; set; }

    public int IdUser { get; set; }

    public DateTime Dt { get; set; }

    public bool Correct { get; set; }

    public virtual PartyQuestionPqt IdPartyQuestionNavigation { get; set; }

    public virtual UserUsr IdUserNavigation { get; set; }
}
