using System;
using System.Collections.Generic;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Entities;

public partial class PartyPty
{
    public Guid Id { get; set; }

    public DateTime Dt { get; set; }

    public string Code { get; set; }

    public int IdPartyType { get; set; }

    public int IdUserHost { get; set; }

    public bool Active { get; set; }

    public bool Finish { get; set; }

    public virtual PartyTypePty IdPartyTypeNavigation { get; set; }

    public virtual UserUsr IdUserHostNavigation { get; set; }

    public virtual ICollection<PartyDifficultyPdf> PartyDifficultyPdf { get; set; } = new List<PartyDifficultyPdf>();

    public virtual ICollection<PartyQuestionPqt> PartyQuestionPqt { get; set; } = new List<PartyQuestionPqt>();

    public virtual ICollection<PartyThemePth> PartyThemePth { get; set; } = new List<PartyThemePth>();

    public virtual ICollection<PartyUserPus> PartyUserPus { get; set; } = new List<PartyUserPus>();
}
