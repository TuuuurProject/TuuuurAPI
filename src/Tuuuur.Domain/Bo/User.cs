using System.Text.Json.Serialization;

namespace Tuuuur.Domain.Bo;

public record User : IBOEntity
{
    public int Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string NickName { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public string Email { get; set; }

    [JsonIgnore]
    public string Password { get; set; }
    public bool IsAdmin { get; set; }
}