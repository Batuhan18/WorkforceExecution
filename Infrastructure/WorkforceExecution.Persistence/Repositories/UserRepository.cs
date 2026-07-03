using Microsoft.EntityFrameworkCore;
using WorkforceExecution.Application.Interfaces;
using WorkforceExecution.Domain.Entities;
using WorkforceExecution.Persistence.Context;

namespace WorkforceExecution.Persistence.Repositories;

public class UserRepository : Repository<AppUser>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct = default)
        => Context.Users
            .Include(u => u.Location)
            .Include(u => u.CrewRegion)
            .FirstOrDefaultAsync(u => u.Email == email, ct);
}
