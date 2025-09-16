using System.Text.Json.Serialization;

namespace Tuuuur.Domain.Bo;

public record User : IBOEntity
{
    public int Id { get; set; }
    public string NickName { get; set; }
    public string Email { get; set; }

    [JsonIgnore]
    public string Password { get; set; }
    
    public byte[] Avatar { get; set; }

    public Guid? ResetPasswordCode { get; set; }
    
    public bool IsNew { get; set; }
    public bool IsAdmin { get; set; }
}