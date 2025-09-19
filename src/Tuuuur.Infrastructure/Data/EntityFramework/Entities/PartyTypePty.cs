using System;
using System.Collections.Generic;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Entities;

public partial class PartyTypePty
{
    public int Id { get; set; }

    public string Label { get; set; }

    public virtual ICollection<PartyPty> PartyPty { get; set; } = new List<PartyPty>();
}
