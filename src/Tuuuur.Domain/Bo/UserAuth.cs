using System.Text.Json.Serialization;

namespace Tuuuur.Domain.Bo;

public record UserAuth : IBOEntity
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public string Code { get; set; }

    public DateTime ExpiresAt { get; set; }

    public virtual User User { get; set; }
}