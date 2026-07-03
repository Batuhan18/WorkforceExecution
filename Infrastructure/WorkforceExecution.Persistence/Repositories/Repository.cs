using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WorkforceExecution.Application.Interfaces;
using WorkforceExecution.Domain.Entities;
using WorkforceExecution.Persistence.Context;

namespace WorkforceExecution.Persistence.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext Context;

    public Repository(AppDbContext context) => Context = context;

    public Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => Context.Set<T>().FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<List<T>> GetAllAsync(CancellationToken ct = default)
        => Context.Set<T>().AsNoTracking().ToListAsync(ct);

    public Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => Context.Set<T>().Where(predicate).ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await Context.Set<T>().AddAsync(entity, ct);

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        => await Context.Set<T>().AddRangeAsync(entities, ct);

    public void Update(T entity) => Context.Set<T>().Update(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => Context.SaveChangesAsync(ct);
}
