using WorkforceExecution.Domain.Entities;

namespace WorkforceExecution.Application.Interfaces;

public interface IUserRepository : IRepository<AppUser>
{
    Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct = default);
}
