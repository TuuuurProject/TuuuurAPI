using System;
using System.Collections.Generic;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Entities;

public partial class User_USR
{
    public int Id { get; set; }

    public string NickName { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }

    public byte[] Avatar { get; set; }

    public Guid? ResetPasswordCode { get; set; }

    public bool IsAdmin { get; set; }

    public bool IsNew { get; set; }

    public virtual ICollection<UserAuth_UAT> UserAuth_UAT { get; set; } = new List<UserAuth_UAT>();
}
