using MediatR;
using WorkforceExecution.Application.Common;
using WorkforceExecution.Application.Dtos;
using WorkforceExecution.Application.Interfaces;

namespace WorkforceExecution.Application.Features.Reports.Queries;

// Onaylanan kayitlar Daily Report'ta konsolide edilir; KPI'lar bu veriden hesaplanir.
public class GetDailyReportQuery : IRequest<Result<DailyReportDto>>
{
    public DateTime Date { get; set; }
}

public class DailyReportDto
{
    public DateTime Date { get; set; }
    public List<WorkItemDto> Rows { get; set; } = new();
    public KpiDto Kpis { get; set; } = new();
}

public class KpiDto
{
    public decimal TotalPlannedQuantity { get; set; }
    public decimal TotalFactQuantity { get; set; }
    public decimal TotalPlannedManday { get; set; }
    public decimal TotalFactManday { get; set; }
    public decimal TotalOvertime { get; set; }
    public decimal? OverallEfficiency { get; set; } // Fact / Plan (%)
    public int ApprovedCount { get; set; }
}

public class GetDailyReportQueryHandler : IRequestHandler<GetDailyReportQuery, Result<DailyReportDto>>
{
    private readonly IWorkItemRepository _workItems;

    public GetDailyReportQueryHandler(IWorkItemRepository workItems) => _workItems = workItems;

    public async Task<Result<DailyReportDto>> Handle(GetDailyReportQuery request, CancellationToken ct)
    {
        var approved = await _workItems.GetApprovedAsync(request.Date.Date, ct);
        var rows = approved.Select(WorkItemMapper.ToDto).ToList();

        // Negatif fact (-1) 'is yapilmadi' anlamina gelir; toplamlara katilmaz.
        var plannedQty = rows.Sum(r => r.PlannedQuantity);
        var factQty = rows.Where(r => r.FactQuantity is > 0).Sum(r => r.FactQuantity!.Value);

        var kpis = new KpiDto
        {
            TotalPlannedQuantity = plannedQty,
            TotalFactQuantity = factQty,
            TotalPlannedManday = rows.Sum(r => r.PlannedManday),
            TotalFactManday = rows.Where(r => r.FactManday is > 0).Sum(r => r.FactManday!.Value),
            TotalOvertime = rows.Where(r => r.Overtime is > 0).Sum(r => r.Overtime!.Value),
            OverallEfficiency = plannedQty > 0 ? Math.Round(factQty / plannedQty * 100, 1) : null,
            ApprovedCount = rows.Count
        };

        return Result<DailyReportDto>.Success(new DailyReportDto
        {
            Date = request.Date.Date,
            Rows = rows,
            Kpis = kpis
        });
    }
}
