using System;
using System.Collections.Generic;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Entities;

public partial class UserAuth_UAT
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Code { get; set; }

    public DateTime ExpiresAt { get; set; }

    public virtual User_USR User { get; set; }
}
