using Microsoft.EntityFrameworkCore;
using WorkforceExecution.Application.Interfaces;
using WorkforceExecution.Domain.Entities;
using WorkforceExecution.Domain.Enums;
using WorkforceExecution.Persistence.Context;

namespace WorkforceExecution.Persistence.Repositories;

public class LookupRepository : ILookupRepository
{
    private readonly AppDbContext _context;

    public LookupRepository(AppDbContext context) => _context = context;

    public Task<List<Location>> GetLocationsAsync(int? crewRegionId, CancellationToken ct = default)
    {
        var query = _context.Locations.Include(l => l.CrewRegion).AsNoTracking();
        if (crewRegionId.HasValue) query = query.Where(l => l.CrewRegionId == crewRegionId.Value);
        return query.OrderBy(l => l.Code).ToListAsync(ct);
    }

    public Task<List<Project>> GetProjectsAsync(int? locationId, CancellationToken ct = default)
    {
        var query = _context.Projects.AsNoTracking();
        if (locationId.HasValue) query = query.Where(p => p.LocationId == locationId.Value);
        return query.OrderBy(p => p.Code).ToListAsync(ct);
    }

    public Task<List<TypeOfWork>> GetWorkCatalogAsync(CancellationToken ct = default)
        => _context.TypeOfWorks
            .Include(t => t.SubTypes).ThenInclude(s => s.SubSubTypes)
            .AsNoTracking()
            .OrderBy(t => t.Code)
            .ToListAsync(ct);

    public Task<List<WorkerType>> GetWorkerTypesAsync(CancellationToken ct = default)
        => _context.WorkerTypes.AsNoTracking().OrderBy(w => w.Name).ToListAsync(ct);

    public Task<List<AppUser>> GetHeadOfMastersAsync(int? locationId, CancellationToken ct = default)
    {
        var query = _context.Users.AsNoTracking().Where(u => u.Role == UserRole.HeadOfMaster);
        if (locationId.HasValue) query = query.Where(u => u.LocationId == locationId.Value);
        return query.OrderBy(u => u.Email).ToListAsync(ct);
    }
}
