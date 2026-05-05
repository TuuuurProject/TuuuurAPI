using System;
using System.Collections.Generic;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Entities;

public partial class UserAuthUat
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public string Code { get; set; }

    public DateTime ExpiresAt { get; set; }

    public virtual UserUsr User { get; set; }
}
