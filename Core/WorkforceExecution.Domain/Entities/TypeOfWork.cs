namespace WorkforceExecution.Domain.Entities;

public class TypeOfWork : BaseEntity
{
    public string Code { get; set; } = null!; // TOW-01
    public ICollection<SubTypeOfWork> SubTypes { get; set; } = new List<SubTypeOfWork>();
}
