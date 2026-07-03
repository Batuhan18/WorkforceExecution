using MediatR;
using WorkforceExecution.Application.Common;
using WorkforceExecution.Application.Interfaces;

namespace WorkforceExecution.Application.Features.Lookups.Queries;

// Plan/crew ekranlarinin ihtiyaci olan tum referans verileri tek istekte doner.
public class GetLookupsQuery : IRequest<Result<LookupsDto>>
{
    public int? CrewRegionId { get; set; } // Teknik Ofis sadece kendi bolgesini gorur
}

public class LookupsDto
{
    public List<LocationDto> Locations { get; set; } = new();
    public List<ProjectDto> Projects { get; set; } = new();
    public List<TowDto> WorkCatalog { get; set; } = new();
    public List<WorkerTypeDto> WorkerTypes { get; set; } = new();
}

public class LocationDto { public int Id { get; set; } public string Code { get; set; } = null!; public string Region { get; set; } = null!; }
public class ProjectDto { public int Id { get; set; } public string Code { get; set; } = null!; public int LocationId { get; set; } }
public class WorkerTypeDto { public int Id { get; set; } public string Name { get; set; } = null!; }
public class TowDto
{
    public string Code { get; set; } = null!;
    public List<StowDto> SubTypes { get; set; } = new();
}
public class StowDto
{
    public string Code { get; set; } = null!;
    public List<SstowDto> SubSubTypes { get; set; } = new();
}
public class SstowDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Unit { get; set; } = null!;
    public string TypeOfCode { get; set; } = null!;
    public string? Zzz { get; set; }
}

public class GetLookupsQueryHandler : IRequestHandler<GetLookupsQuery, Result<LookupsDto>>
{
    private readonly ILookupRepository _lookups;

    public GetLookupsQueryHandler(ILookupRepository lookups) => _lookups = lookups;

    public async Task<Result<LookupsDto>> Handle(GetLookupsQuery request, CancellationToken ct)
    {
        var locations = await _lookups.GetLocationsAsync(request.CrewRegionId, ct);
        var projects = await _lookups.GetProjectsAsync(null, ct);
        var catalog = await _lookups.GetWorkCatalogAsync(ct);
        var workerTypes = await _lookups.GetWorkerTypesAsync(ct);

        var locationIds = locations.Select(l => l.Id).ToHashSet();

        return Result<LookupsDto>.Success(new LookupsDto
        {
            Locations = locations.Select(l => new LocationDto { Id = l.Id, Code = l.Code, Region = l.CrewRegion.Name }).ToList(),
            Projects = projects.Where(p => locationIds.Contains(p.LocationId))
                               .Select(p => new ProjectDto { Id = p.Id, Code = p.Code, LocationId = p.LocationId }).ToList(),
            WorkerTypes = workerTypes.Select(w => new WorkerTypeDto { Id = w.Id, Name = w.Name }).ToList(),
            WorkCatalog = catalog.Select(t => new TowDto
            {
                Code = t.Code,
                SubTypes = t.SubTypes.OrderBy(s => s.Code).Select(s => new StowDto
                {
                    Code = s.Code,
                    SubSubTypes = s.SubSubTypes.OrderBy(x => x.Code).Select(x => new SstowDto
                    {
                        Id = x.Id, Code = x.Code, Unit = x.Unit, TypeOfCode = x.TypeOfCode, Zzz = x.Zzz
                    }).ToList()
                }).ToList()
            }).ToList()
        });
    }
}
