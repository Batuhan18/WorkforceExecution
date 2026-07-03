namespace WorkforceExecution.Domain.Entities;

public class SubSubTypeOfWork : BaseEntity
{
    public string Code { get; set; } = null!;     // SSTOW-01
    public string Unit { get; set; } = null!;     // m3, m2, t, -
    public string TypeOfCode { get; set; } = null!; // AAA01002026
    public string? Zzz { get; set; }              // ZZZ detay kodu
    public int SubTypeOfWorkId { get; set; }
    public SubTypeOfWork SubTypeOfWork { get; set; } = null!;
}
