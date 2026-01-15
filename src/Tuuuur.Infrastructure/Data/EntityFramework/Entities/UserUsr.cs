using System;
using System.Collections.Generic;

namespace Tuuuur.Infrastructure.Data.EntityFramework.Entities;

public partial class UserUsr
{
    public int Id { get; set; }

    public string NickName { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }

    public string Avatar { get; set; }

    public Guid? ResetPasswordCode { get; set; }

    public bool IsAdmin { get; set; }

    public bool IsNew { get; set; }

    public bool IsGoogleUser { get; set; }

    public virtual ICollection<EloElo> EloElo { get; set; } = new List<EloElo>();

    public virtual ICollection<PartyPty> PartyPty { get; set; } = new List<PartyPty>();

    public virtual ICollection<PartyUserPus> PartyUserPus { get; set; } = new List<PartyUserPus>();

    public virtual RefreshTokenRtk RefreshTokenRtk { get; set; }

    public virtual ICollection<UserAuthUat> UserAuthUat { get; set; } = new List<UserAuthUat>();

    public virtual ICollection<UserPartyQuestionUpq> UserPartyQuestionUpq { get; set; } = new List<UserPartyQuestionUpq>();
}
