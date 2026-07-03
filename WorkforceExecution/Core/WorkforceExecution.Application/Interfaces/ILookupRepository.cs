using WorkforceExecution.Domain.Entities;

namespace WorkforceExecution.Application.Interfaces;

// Plan olusturma ekraninin ihtiyac duydugu referans verileri tek noktadan verir.
public interface ILookupRepository
{
    Task<List<Location>> GetLocationsAsync(int? crewRegionId, CancellationToken ct = default);
    Task<List<Project>> GetProjectsAsync(int? locationId, CancellationToken ct = default);
    Task<List<TypeOfWork>> GetWorkCatalogAsync(CancellationToken ct = default);
    Task<List<WorkerType>> GetWorkerTypesAsync(CancellationToken ct = default);
    Task<List<AppUser>> GetHeadOfMastersAsync(int? locationId, CancellationToken ct = default);
}
