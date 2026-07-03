using WorkforceExecution.Domain.Entities;

namespace WorkforceExecution.Application.Interfaces;

public interface ITokenService
{
    string CreateToken(AppUser user);
}
