using System;
using System.Collections.Generic;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Entities;

public partial class PartyQuestionPqt
{
    public int Id { get; set; }

    public int IdQuestion { get; set; }

    public Guid IdParty { get; set; }

    public int Order { get; set; }

    public virtual PartyPty IdPartyNavigation { get; set; }

    public virtual QuestionQst IdQuestionNavigation { get; set; }

    public virtual ICollection<UserPartyQuestionUpq> UserPartyQuestionUpq { get; set; } = new List<UserPartyQuestionUpq>();
}
