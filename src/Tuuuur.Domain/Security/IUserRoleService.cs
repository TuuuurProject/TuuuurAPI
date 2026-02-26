namespace Tuuuur.Domain.Security;

public interface IUserRoleService
{
    string GetEmail();
    Guid GetUserId();
}