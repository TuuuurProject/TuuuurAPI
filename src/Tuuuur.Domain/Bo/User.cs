using System.Text.Json.Serialization;

namespace Tuuuur.Domain.Bo;

public record User : IBOEntity
{
    public Guid Id { get; set; }

    public string NickName { get; set; }

    public string Email { get; set; }

    [JsonIgnore]
    public string Password { get; set; }

    public string Avatar { get; set; }
    
    [JsonIgnore]
    public Guid? ResetPasswordCode { get; set; }
    
    public bool IsActive { get; set; }

    public bool IsAdmin { get; set; }

    public bool IsNew { get; set; }
    
    public bool IsGoogleUser { get; set; }
    public bool IsInvitedUser { get; set; } = false;
    
    public List<UserAuth> UserAuth { get; set; } = [];
    
    public List<Elo> Elo { get; set; } = [];
    
    public int GlobalElo => Elo.Count != 0 ? Elo.Sum(p_P => p_P.Value) / Elo.Count : 0;
}