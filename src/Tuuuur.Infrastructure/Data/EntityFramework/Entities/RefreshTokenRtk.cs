using System;
using System.Collections.Generic;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Entities;

public partial class RefreshTokenRtk
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Token { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public bool IsRevoked { get; set; }

    public virtual UserUsr User { get; set; }
}
