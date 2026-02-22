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

    public bool IsAdmin { get; set; }

    public bool IsNew { get; set; }
    
    public bool IsGoogleUser { get; set; }
    
    public virtual ICollection<UserAuth> UserAuth { get; set; } = new List<UserAuth>();
}